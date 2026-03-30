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

public class TunToNodePump : ITunToNodePump {

    private readonly IConnectionHealthSink healthSink;
    private readonly GeneralSettings settings;
    private readonly IConnectionManager connectionManager;
    private CancellationTokenSource? cancellationToken;
    Task? tunLissen;
    public TunToNodePump (
            IConnectionHealthSink healthSink,
            IOptions<GeneralSettings> options,
            IConnectionManager connectionManager) {
        this.healthSink = healthSink;
        this.settings = options.Value;
        this.connectionManager = connectionManager;
    }

    public async Task StartAsync (CancellationToken ct) {
        cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var winDivertObj = new WinDivertObject(settings.TcpProxyPort);

        if (!CreateHandle(winDivertObj.outboundFilter, out var hendl)){
            throw new InvalidOperationException("Не удалось открыть WinDivert хэндл");
        }
        var lostReason = ConnectionLostReason.UserClosed;
        try {
            tunLissen = Task.Run(
                () => TunToNodeLoop(cancellationToken.Token, winDivertObj, hendl),
                cancellationToken.Token);
        } catch {
            lostReason = ConnectionLostReason.TransportError;
        } finally {
            WinDivert.WinDivertClose(hendl);
            healthSink.OnConnectionLost(lostReason, ct);
        }
    }

    public async Task StopAsync () {
        cancellationToken?.Cancel();
        if (tunLissen != null) {
            await tunLissen;
        }
    }

    private bool CreateHandle (string filter, out IntPtr hendl) {
        hendl = WinDivert
                .WinDivertOpen(filter, WinDivertLayer.Network, 0, WinDivertOpenFlags.None);
         return (hendl != IntPtr.Zero && hendl != (IntPtr)(-1));
    }

    private void TunToNodeLoop (CancellationToken ct, WinDivertObject winDivertObj, IntPtr hendl) {
        WinDivertAddress winDivAddr = new ();
        while (!ct.IsCancellationRequested) {
            uint recvLen = 0;
            if (!WinDivert.WinDivertRecv(hendl, winDivertObj.buffer, ref winDivAddr, ref recvLen)) {
                throw new Exception();
            }
            var p = WinDivert
                .WinDivertHelperParsePacket(winDivertObj.buffer, recvLen);
            if (p is null || p.PacketPayloadLength == 0) {
                WinDivert
                    .WinDivertSend(hendl, winDivertObj.buffer, recvLen, ref winDivAddr);
                continue;
            }
            unsafe {
                bool modified = false;
                string dstAddr = string.Empty;
                if (p.IPv4Header != null) {
                    dstAddr = p.IPv4Header->DstAddr.ToString();
                    p.IPv4Header->DstAddr = IPAddress.Loopback;
                    modified = true;
                }
                if (modified) {
                    if (p.TcpHeader != null) {
                        connectionManager
                            .AddTсpRoute(
                                p.TcpHeader->SrcPort,
                                (dstAddr, p.TcpHeader->DstPort));
                        p.TcpHeader->DstPort = settings.TcpProxyPort;
                    } else if (p.UdpHeader != null) {
                        connectionManager.AddUdpRoute(
                                p.UdpHeader->SrcPort,
                                (dstAddr, p.UdpHeader->DstPort)
                            );
                        p.UdpHeader->DstPort = settings.UdpProxyPort;
                    }
                    WinDivert
                        .WinDivertHelperCalcChecksums(winDivertObj.buffer, recvLen, ref winDivAddr, 0);
                }
                WinDivert.WinDivertSend(hendl, winDivertObj.buffer, recvLen, ref winDivAddr);
            }
        }
    }
}
