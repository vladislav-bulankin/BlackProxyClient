using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Exceptions;
using BlackTunnel.Domain.Runtime;
using System.Net;
using System.Net.Sockets;

namespace BlackTunnel.Core.DataPlane;

public class UdpTunnelHandler : IUdpTunnelHandler {
    private UdpClient? listener;           // слушает от WinDivert / приложения
    private UdpClient? relayClient;        // постоянное соединение к Node UDP-relay
    private bool isInitialized;
    private SessionContext? context;
    private readonly IConnectionHealthSink connectionHealthSink;
    private readonly IConnectionManager connectionManager;

    private CancellationTokenSource? receiverCts;
    private Task? receiverTask;

    public UdpTunnelHandler (
        IConnectionHealthSink connectionHealthSink,
        IConnectionManager connectionManager) {
        this.connectionHealthSink = connectionHealthSink;
        this.connectionManager = connectionManager;
    }

    public void Initialize (int udpProxyPort) {
        if (isInitialized) { return; }
        listener = new UdpClient(new IPEndPoint(IPAddress.Loopback, udpProxyPort));
        isInitialized = true;
    }

    public async Task StartAsync (SessionContext context, CancellationToken ct) {
        if (!isInitialized || listener is null) { return; }
        this.context = context;
        // Запускаем background receiver ответов от relay
        receiverCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        receiverTask = RunRelayReceiverAsync(receiverCts.Token);

        await AcceptLoopAsync(ct);
    }

    private async Task AcceptLoopAsync (CancellationToken ct) {
        while (!ct.IsCancellationRequested) {
            try {
                var received = await listener!.ReceiveAsync(ct);
                _ = Task.Run(() => HandleUdpAsync(received, ct), ct);
            } catch (OperationCanceledException) {
                break;
            } catch {
                connectionHealthSink.OnConnectionLost(ConnectionLostReason.TransportError, ct);
                break;
            }
        }
    }

    private async Task HandleUdpAsync (UdpReceiveResult received, CancellationToken ct) {
        if (context?.UdpRelayPort == 0) {
            await Task.Delay(300, ct); // ждём, пока OpenUdpAssociateAsync отработает
            if (context.UdpRelayPort == 0) { return; }   
        }
        try {
            var route = ResolveOriginalDst(received);
            var wrapped = WrapWithSocks5Header(received.Buffer, route.RemoteEndpoint!);
            await relayClient!.SendAsync(wrapped, context.Node.NodeHost!, context.UdpRelayPort, ct);
        } catch (Exception) {
            connectionHealthSink.OnConnectionLost(ConnectionLostReason.TransportError, ct);
        }
    }

    // Background приём ответов от UDP-relay
    private async Task RunRelayReceiverAsync (CancellationToken ct) {
        if (relayClient is null) {
            relayClient = new UdpClient();
        }
        while (!ct.IsCancellationRequested) {
            try {
                var response = await relayClient.ReceiveAsync(ct);
                var unwrapped = UnwrapSocks5Header(response.Buffer);
                if (unwrapped is null) { continue; }
                var route = connectionManager.GetUdpRoute((ushort)response.RemoteEndPoint.Port);
                if (route?.LocalEndpoint is not null) {
                    await listener!.SendAsync(unwrapped, route.LocalEndpoint, ct);
                }
            } catch (OperationCanceledException) {
                break;
            } catch {
                // Не падаем полностью при ошибке одного пакета
                await Task.Delay(10, ct);
            }
        }
    }

    private UdpRoute ResolveOriginalDst (UdpReceiveResult received) {
        var srcPort = (ushort)received.RemoteEndPoint.Port;
        return connectionManager.GetUdpRoute(srcPort)
            ?? throw new ProxyNegotiateException($"Маршрут не найден для порта {srcPort}");
    }

    private byte[] WrapWithSocks5Header (byte[] data, IPEndPoint dst) {
        var ip = dst.Address.GetAddressBytes(); // IPv4
        var port = (ushort)dst.Port;

        var header = new byte[] {
            0, 0,           // RSV
            0,              // FRAG
            0x01,           // ATYP = IPv4
            ip[0], ip[1], ip[2], ip[3],
            (byte)(port >> 8), (byte)(port & 0xFF)
        };

        var result = new byte[header.Length + data.Length];
        Buffer.BlockCopy(header, 0, result, 0, header.Length);
        Buffer.BlockCopy(data, 0, result, header.Length, data.Length);
        return result;
    }

    private byte[]? UnwrapSocks5Header (byte[] packet) {
        if (packet.Length < 10 || packet[3] != 0x01) { return null; }
        // Пропускаем: RSV(2) + FRAG(1) + ATYP(1) + IP(4) + PORT(2) = 10 байт
        return packet[10..];
    }

    public void Dispose () {
        receiverCts?.Cancel();
        receiverTask?.Wait(1000);
        listener?.Close();
        relayClient?.Close();
    }
}
