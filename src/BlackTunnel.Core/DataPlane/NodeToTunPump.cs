using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Runtime;
using BlackTunnel.Domain.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using WinDivertSharp;

namespace BlackTunnel.Core.DataPlane;

public class NodeToTunPump : INodeToTunPump {

    private readonly IConnectionManager connectionManager;
    private readonly IConnectionHealthSink healthSink;
    private readonly GeneralSettings settings;
    private CancellationTokenSource? cts;
    private Task? pumpTask;

    public NodeToTunPump (
            IConnectionManager connectionManager, 
            IConnectionHealthSink healthSink,
            IOptions<GeneralSettings> options) {
        this.connectionManager = connectionManager;
        this.healthSink = healthSink;
        this.settings = options.Value;
    }

    public async Task StartAsync (RuntimeContext ctx, CancellationToken ct) {
        cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var winDivertObj = new WinDivertObject(settings.TcpProxyPort);

        if (!CreateHandle(winDivertObj.outboundFilter, out var hendl)) {
            throw new InvalidOperationException("Не удалось открыть WinDivert хэндл");
        }

        var lostReason = ConnectionLostReason.UserClosed;
        try {
            pumpTask = Task.Run(
                () => NodeToTunLoop(cts.Token, winDivertObj, hendl),
                cts.Token);
        } catch {
            lostReason = ConnectionLostReason.TransportError;
        } finally {
            WinDivert.WinDivertClose(hendl);
            healthSink.OnConnectionLost(lostReason, ct);
        }
    }

    public async Task StopAsync () {
        cts?.Cancel();
        if (pumpTask != null) {
            await pumpTask;
        }
    }

    private bool CreateHandle (string filter, out IntPtr hendl) {
        hendl = WinDivert
                .WinDivertOpen(filter, WinDivertLayer.Network, 0, WinDivertOpenFlags.None);
        return (hendl != IntPtr.Zero && hendl != (IntPtr)(-1));
    }

    private void NodeToTunLoop (CancellationToken ct, WinDivertObject obj, IntPtr hendl) {
        WinDivertAddress addr = new();
        while (!ct.IsCancellationRequested) {
            uint len = 0;
            if (!WinDivert.WinDivertRecv(hendl, obj.buffer, ref addr, ref len)) {
                if (!ct.IsCancellationRequested){
                    healthSink.OnConnectionLost(ConnectionLostReason.TransportError, ct);
                }
                break;
            }
            unsafe {
                var p = WinDivert.WinDivertHelperParsePacket(obj.buffer, len);
                if (p.IPv4Header == null || p.UdpHeader == null) {
                    WinDivert.WinDivertSend(hendl, obj.buffer, len, ref addr);
                    continue;
                }
                var route = connectionManager.GetUdpRoute(p.UdpHeader->DstPort);
                if (route is null) { continue; }
                p.IPv4Header->SrcAddr = route.RemoteEndpoint!.Address;
                p.UdpHeader->SrcPort = (ushort)route.RemoteEndpoint.Port;
                p.IPv4Header->DstAddr = IPAddress.Loopback;
                WinDivert.WinDivertHelperCalcChecksums(obj.buffer, len, ref addr, 0);
            }
            WinDivert.WinDivertSend(hendl, obj.buffer, len, ref addr);
        }
    }
}
