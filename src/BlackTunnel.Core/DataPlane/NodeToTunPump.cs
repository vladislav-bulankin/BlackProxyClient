using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Core.Managers.Models;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Runtime;
using BlackTunnel.Domain.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using WinDivertSharp;

namespace BlackTunnel.Core.DataPlane;

public class NodeToTunPump : INodeToTunPump {

    private readonly IRouteTable routeTable;
    private readonly IConnectionHealthSink healthSink;
    private readonly GeneralSettings settings;
    private CancellationTokenSource? cts;
    private Task? pumpTask;

    public NodeToTunPump (
            IRouteTable routeTable, 
            IConnectionHealthSink healthSink,
            IOptions<GeneralSettings> options) {
        this.routeTable = routeTable;
        this.healthSink = healthSink;
        this.settings = options.Value;
    }

    public async Task StartAsync (SessionContext ctx, CancellationToken ct) {
        cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var winDivertObj = new WinDivertObject(settings.TcpProxyPort);

        if (!CreateHandle(winDivertObj.outboundFilter, out var hendl)) {
            throw new InvalidOperationException("Не удалось открыть WinDivert хэндл");
        }
        pumpTask = Task.Run(
            () => NodeToTunLoop(cts.Token, winDivertObj, hendl),
            cts.Token);
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
        var lostReason = ConnectionLostReason.UserClosed;
        WinDivertAddress addr = new();
        try {
            while (!ct.IsCancellationRequested) {
                uint len = 0;
                if (!WinDivert.WinDivertRecv(hendl, obj.buffer, ref addr, ref len)) {
                    if (!ct.IsCancellationRequested) {
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
                    routeTable.TryGetUdpRoute(p.UdpHeader->DstPort, out var route);
                    if (route is null) { continue; }
                    p.IPv4Header->SrcAddr = route.RemoteEndpoint!.Address;
                    p.UdpHeader->SrcPort = (ushort)route.RemoteEndpoint.Port;
                    p.IPv4Header->DstAddr = IPAddress.Loopback;
                    WinDivert.WinDivertHelperCalcChecksums(obj.buffer, len, ref addr, 0);
                }
                WinDivert.WinDivertSend(hendl, obj.buffer, len, ref addr);
            }
        } catch {
            lostReason = ConnectionLostReason.TransportError;
        } finally {
            WinDivert.WinDivertClose(hendl);
            healthSink.OnConnectionLost(lostReason, ct);
        }
    }
}
