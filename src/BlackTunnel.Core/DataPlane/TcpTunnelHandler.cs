using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Core.Exceptions;
using BlackTunnel.Core.Managers.Models;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Runtime;
using System.Net;
using System.Net.Sockets;

namespace BlackTunnel.Core.DataPlane; 
public class TcpTunnelHandler : ITcpTunnelHandler {
    private readonly IRouteTable routeTable;
    private readonly IConnectionHealthSink connectionHealthSink;
    private readonly IMuxConnection muxConnection;
    private TcpListener listener;
    private bool isInitialized;
    private SessionContext context;
    public TcpTunnelHandler (
            IRouteTable routeTable,
            IConnectionHealthSink connectionHealthSink,
            IMuxConnection muxConnection) {
        this.routeTable = routeTable;
        this.connectionHealthSink = connectionHealthSink;
        this.muxConnection = muxConnection;
    }

    public void Initialize (int tcpProxyPort) {
        if (!isInitialized) {
            listener = new TcpListener(IPAddress.Loopback, tcpProxyPort);
            isInitialized = true;
        }
    }

    public async Task StartAsync (SessionContext context, CancellationToken ct) {
        if (!isInitialized) { return; }
        this.context = context;
        listener.Start();
        _ = Task.Run(() => OpenUdpAssociateAsync(ct), ct);
        await AcceptLoopAsync(ct);
    }

    private async Task OpenUdpAssociateAsync (CancellationToken ct) {
        try {
            var udpAssoc = await muxConnection.OpenStreamAsync(
                new IPEndPoint(IPAddress.Any, 0), ct);
            var response = await udpAssoc.ReadAsync(ct);
            context.UdpRelayPort = (response[0] << 8) | response[1];
        } catch {
            connectionHealthSink.OnConnectionLost(
                ConnectionLostReason.NegotiationFailed, ct);
        }
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
                var muxStream = await muxConnection.OpenStreamAsync(originalDst, ct);
                var clientStream = client.GetStream();
                var toMux = PumpClientToMuxAsync(clientStream, muxStream, ct);
                var fromMux = PumpMuxToClientAsync(muxStream, clientStream, ct);
                await Task.WhenAll(toMux, fromMux);
                await muxStream.CloseAsync(ct);
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

        _ = routeTable.TryGetOriginalTcpDest((ushort)srcEndPoint.Port, out var dest);
            return dest ?? throw new ProxyNegotiateException(
                $"Маршрут не найден для порта {srcEndPoint.Port}");
    }

    private static async Task PumpClientToMuxAsync (
    NetworkStream clientStream,
    MuxStream muxStream,
    CancellationToken ct) {
        var buffer = new byte[16 * 1024];
        while (!ct.IsCancellationRequested) {
            var bytesRead = await clientStream.ReadAsync(buffer.AsMemory(), ct);
            if (bytesRead == 0) { break; } 
            await muxStream.WriteAsync(buffer.AsSpan(0, bytesRead).ToArray(), ct);
        }
    }

    private static async Task PumpMuxToClientAsync (
        MuxStream muxStream,
        NetworkStream clientStream,
        CancellationToken ct) {
        while (!ct.IsCancellationRequested) {
            try {
                var data = await muxStream.ReadAsync(ct);
                if (data.Length == 0) { break; }
                await clientStream.WriteAsync(data, ct);
            } catch (Exception) when (ct.IsCancellationRequested) { break; }
        }
    }
}
