using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Core.Exceptions;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Runtime;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BlackTunnel.Core.DataPlane; 
public class TcpTunnelHandler : ITcpTunnelHandler {
    IConnectionManager connectionManager;
    private readonly IConnectionHealthSink connectionHealthSink;
    private TcpListener listener;
    private bool isInitialized;
    private RuntimeContext context;
    public TcpTunnelHandler (
            IConnectionManager connectionManager,
            IConnectionHealthSink connectionHealthSink) {
        this.connectionManager = connectionManager;
        this.connectionHealthSink = connectionHealthSink;
    }

    public void Initialize (int tcpProxyPort) {
        if (!isInitialized) {
            listener = new TcpListener(IPAddress.Loopback, tcpProxyPort);
            isInitialized = true;
        }
    }

    public async Task StartAsync (RuntimeContext context, CancellationToken ct) {
        if (!isInitialized) { return; }
        this.context = context;
        listener.Start();
        _ = Task.Run(() => OpenUdpAssociateAsync(ct), ct);
        await AcceptLoopAsync(ct);
    }

    private async Task OpenUdpAssociateAsync (CancellationToken ct) {
        using var socket = new TcpClient { NoDelay = true };
        try {
            await socket.ConnectAsync(context.NodeHost!, context.NodePort, ct);
            var stream = socket.GetStream();
            await AuthenticateAsync(stream, ct);
            var request = new byte[] {
                5, 3, 0, 1,      
                0, 0, 0, 0,       
                0, 0             
            };
            await stream.WriteAsync(request, ct);
            var response = new byte[10];
            await ReadExactAsync(stream, response, 10, ct);
            if (response[1] != 0x00) {
                throw new ProxyNegotiateException(
                    $"UDP ASSOCIATE отклонён: 0x{response[1]:X2}");
            }
            context.UdpRelayPort = (response[8] << 8) | response[9];
            // Если TCP закроется, сервер закроет UDP relay
            await WatchControlAsync(socket, ct);
        } catch {
            connectionHealthSink.OnConnectionLost(
                ConnectionLostReason.NegotiationFailed, ct);
        }
    }

    private async Task WatchControlAsync (TcpClient control, CancellationToken ct) {
        try {
            var buf = new byte[1];
            while (!ct.IsCancellationRequested) {
                if (await control.GetStream().ReadAsync(buf, ct) == 0) {
                    break;
                }
            }
        } catch { }
    }

    private async Task AcceptLoopAsync (CancellationToken ct) {
        while (!ct.IsCancellationRequested) {
            try {
                var client = await listener.AcceptTcpClientAsync(ct);
                _ = Task.Run(() => HandleClientAsync(client, ct), ct);
            } catch (OperationCanceledException) {
                break;
            } catch (Exception) {
                connectionHealthSink.OnConnectionLost(
                    ConnectionLostReason.TransportError, ct);
                listener.Stop();
                isInitialized = false;
                break;
            }
        }
    }

    private async Task HandleClientAsync (TcpClient client, CancellationToken ct) {
        using (client) {
            try {
                var originalDst = ResolveOriginalDst(client);

                using var socket = await ConnectToNodeAsync(ct);
                var nodeStream = socket.GetStream();
                var clientStream = client.GetStream();

                await AuthenticateAsync(nodeStream, ct);
                await Socks5NegotiateAsync(nodeStream, originalDst, ct);
                await RelayAsync(clientStream, nodeStream, ct);
            } catch (ProxyAuthException) {
                connectionHealthSink.OnConnectionLost(
                    ConnectionLostReason.AuthFailed, ct);
            } catch (ProxyNegotiateException) {
                connectionHealthSink.OnConnectionLost(
                    ConnectionLostReason.NegotiationFailed, ct);
            } catch (Exception) {
                connectionHealthSink.OnConnectionLost(
                    ConnectionLostReason.TransportError, ct);
            }
        }
    }

    private IPEndPoint ResolveOriginalDst (TcpClient client) {
        var srcEndPoint = (IPEndPoint?)client.Client.RemoteEndPoint
            ?? throw new ProxyNegotiateException("No RemoteEndPoint");

        return connectionManager.GetOriginalTcpDst((ushort)srcEndPoint.Port)
            ?? throw new ProxyNegotiateException(
                $"Маршрут не найден для порта {srcEndPoint.Port}");
    }

    private async Task<TcpClient> ConnectToNodeAsync (CancellationToken ct) {
        var socket = new TcpClient { NoDelay = true };
        try {
            await socket.ConnectAsync(context.NodeHost!, context.NodePort, ct);
            return socket;
        } catch (Exception ex) {
            socket.Dispose();
            throw new ProxyNegotiateException($"Не удалось подключиться к узлу: {ex.Message}");
        }
    }

    private async Task AuthenticateAsync (NetworkStream stream, CancellationToken ct) {
        var tokenBytes = Encoding.UTF8.GetBytes(context.ConnectionToken
            ?? throw new ProxyAuthException("Токен отсутствует"));

        var tokenLen = (ushort)tokenBytes.Length;
        var request = new byte[3 + tokenBytes.Length];
        request[0] = 0x02;
        request[1] = (byte)(tokenLen >> 8);
        request[2] = (byte)(tokenLen & 0xFF);
        tokenBytes.CopyTo(request, 3);
        await stream.WriteAsync(request, ct);

        // Ответ: [status][err_len][err_bytes?]
        var header = new byte[2];
        await ReadExactAsync(stream, header, 2);

        if (header[1] > 0) {
            var errBytes = new byte[header[1]];
            await ReadExactAsync(stream, errBytes, header[1]);
            throw new ProxyAuthException(Encoding.UTF8.GetString(errBytes));
        }

        if (header[0] != 0x00){
            throw new ProxyAuthException("Авторизация отклонена без причины");
        }
    }

    private async Task Socks5NegotiateAsync (
        NetworkStream stream, IPEndPoint dst, CancellationToken ct) {
        var dstBytes = dst.Address.GetAddressBytes();
        var port = (ushort)dst.Port;
        var request = new byte[] {
            5, 1, 0, 1,
            dstBytes[0], dstBytes[1], dstBytes[2], dstBytes[3],
            (byte)(port >> 8), (byte)(port & 0xFF)
        };
        await stream.WriteAsync(request, ct);

        var response = new byte[10];
        await ReadExactAsync(stream, response, 10);

        if (response[1] != 0x00){
            throw new ProxyNegotiateException(
                $"SOCKS5 CONNECT отклонён: 0x{response[1]:X2}");
        }
    }

    private async Task ReadExactAsync (
            NetworkStream stream, byte[] buffer, 
            int count, CancellationToken ct = default) {
        int total = 0;
        while (total < count) {
            int n = await stream.ReadAsync(buffer.AsMemory(total, count - total), ct);
            if (n == 0)
                throw new ProxyNegotiateException("Соединение разорвано при чтении");
            total += n;
        }
    }

    private async Task RelayAsync (
            NetworkStream client, NetworkStream node, CancellationToken ct) =>
        await Task.WhenAny(
            client.CopyToAsync(node, ct),
            node.CopyToAsync(client, ct)
        );
}
