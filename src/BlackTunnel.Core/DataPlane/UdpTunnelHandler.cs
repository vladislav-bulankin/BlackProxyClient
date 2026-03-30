using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Core.Managers.Models;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Exceptions;
using BlackTunnel.Domain.Runtime;
using System.Net;
using System.Net.Sockets;

namespace BlackTunnel.Core.DataPlane; 
public class UdpTunnelHandler : IUdpTunnelHandler {
    private UdpClient listener;
    private bool isInitialized;
    private RuntimeContext context;
    private readonly IConnectionHealthSink connectionHealthSink;
    private readonly IConnectionManager connectionManager;
    private readonly IMuxConnection muxConnection;
    private short retryCounter = 0;
    private short currentRetry = 5;
    public UdpTunnelHandler (
            IConnectionHealthSink connectionHealthSink,
            IConnectionManager connectionManager,
            IMuxConnection muxConnection) {
        this.connectionHealthSink = connectionHealthSink;
        this.connectionManager = connectionManager;
        this.muxConnection = muxConnection;
    }
    public async Task StartListenAsync (RuntimeContext context, CancellationToken ct) {
        if (!isInitialized) {
            return;
        }
        this.context = context;
        await AcceptLoopAsync(ct);
    }

    public async Task StartLissenAsync (RuntimeContext context, CancellationToken ct) {
        if (!isInitialized) { return; }
        this.context = context;
        await AcceptLoopAsync(ct);
    }

    private async Task AcceptLoopAsync (CancellationToken ct) {
        while (!ct.IsCancellationRequested) {
            try {
                var received = await listener.ReceiveAsync(ct);
                _ = Task.Run(() => HandleUdpAsync(received, ct), ct);
            } catch (OperationCanceledException) {
                break;
            } catch {
                connectionHealthSink.OnConnectionLost(
                    ConnectionLostReason.TransportError, ct);
                break;
            }
        }
    }

    private async Task HandleUdpAsync (UdpReceiveResult received, CancellationToken ct) {
        if (context.UdpRelayPort == 0 && CanRetry()) {
            await Task.Delay(200);
            await HandleUdpAsync(received, ct);
        }
        if(context.UdpRelayPort == 0 && !CanRetry()) { return; }
        try {
            var route = ResolveOriginalDst(received);
            // Оборачиваем в SOCKS5 UDP заголовок и шлём на relay
            var wrapped = WrapWithSocks5Header(received.Buffer, route.RemoteEndpoint!);
            using var udp = new UdpClient();
            await udp.SendAsync(wrapped, context.NodeHost!, context.UdpRelayPort, ct);
            // Получаем ответ от relay
            var response = await udp.ReceiveAsync(ct);
            // Разворачиваем SOCKS5 заголовок
            var unwrapped = UnwrapSocks5Header(response.Buffer);
            if (unwrapped is null) { return; }
            // Шлём обратно на порт приложения
            await listener.SendAsync(unwrapped, route.LocalEndpoint!, ct);
        } catch {
            connectionHealthSink.OnConnectionLost(
                ConnectionLostReason.TransportError, ct);
        }
    }

    private bool CanRetry () {
        if(retryCounter > currentRetry) { return false; }
        currentRetry++;
        return true;
    }

    private UdpRoute ResolveOriginalDst (UdpReceiveResult received) {
        var srcPort = (ushort)received.RemoteEndPoint.Port;
        return connectionManager.GetUdpRoute(srcPort)
            ?? throw new ProxyNegotiateException(
                $"Маршрут не найден для порта {srcPort}");
    }

    private byte[] WrapWithSocks5Header (byte[] data, IPEndPoint dst) {
        var ip = dst.Address.GetAddressBytes(); // IPv4
        var port = (ushort)dst.Port;
        var header = new byte[] {
        0, 0,       // RSV
        0,          // FRAG — не фрагментируем
        0x01,       // ATYP IPv4
        ip[0], ip[1], ip[2], ip[3],
        (byte)(port >> 8), (byte)(port & 0xFF)
    };
        return [.. header, .. data];
    }
    // Разбираем ответ от relay — убираем SOCKS5 заголовок
    private byte[]? UnwrapSocks5Header (byte[] packet) {
        if (packet.Length < 10)
            return null;
        // [RSV(2)][FRAG(1)][ATYP(1)][IP(4)][PORT(2)][DATA]
        if (packet[3] != 0x01)
            return null; // только IPv4
        return packet[10..];                // только данные
    }
}
