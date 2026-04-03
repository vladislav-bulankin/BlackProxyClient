using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Core.Managers.Models;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Runtime;
using BlackTunnel.Domain.Settings;
using Microsoft.Extensions.Options;

namespace BlackTunnel.Core.Managers;

public class ConnectionManager : IConnectionManager {
    private readonly ITunToNodePump tunToNode;
    private readonly INodeToTunPump nodeToTun;
    private readonly ITcpTunnelHandler tcpTunnel;
    private readonly IUdpTunnelHandler udpTunnel;
    private readonly IMuxConnection muxConnection;
    private readonly GeneralSettings settings;
    public ConnectionManager (
            ITunToNodePump tunToNode, 
            INodeToTunPump nodeToTun, 
            ITcpTunnelHandler tcpTunnel, 
            IUdpTunnelHandler udpTunnel,
            IOptions<GeneralSettings> options,
            IMuxConnection muxConnection) {
        this.tunToNode = tunToNode;
        this.nodeToTun = nodeToTun;
        this.tcpTunnel = tcpTunnel;
        this.udpTunnel = udpTunnel;
        this.settings = options.Value;
        this.muxConnection = muxConnection;
    }

    public async Task ConnectAsync (SessionContext session) {
        if(session is null) { 
            throw new ArgumentNullException("not authorieze");
        }
        try {
            await tunToNode.StartAsync(session?.Cts?.Token ?? CancellationToken.None);
            await nodeToTun.StartAsync(session, session?.Cts?.Token ?? CancellationToken.None);
            tcpTunnel.Initialize(settings.TcpProxyPort);
            _ = Task.Run(() => tcpTunnel.StartAsync(session, session.Cts.Token));
            await Task.Delay(TimeSpan.FromSeconds(2));
            udpTunnel.Initialize(settings.UdpProxyPort);
            _ = Task.Run(() => udpTunnel.StartAsync(session, session.Cts.Token));
        } catch (Exception) {
            throw;
        }
    }

    public async Task DisconnectAsync (SessionContext session) {
        if(session  is null) { return; }
        session.Cts.Cancel();
        try {
            await tunToNode.StopAsync();
            await nodeToTun.StopAsync();
            await udpTunnel.StopAsync();
            await tcpTunnel.StopAsync();
        } finally {
            await muxConnection.DisposeAsync();
        }
    }

    public async Task ReconnectAsync (SessionContext session) {
        if(session is null) {
            throw new ArgumentNullException("not authorieze");
        }
        session.Cts.Cancel();
        await muxConnection.DisposeAsync();
        try {
            await tunToNode.StopAsync();
            await nodeToTun.StopAsync();
            await udpTunnel.StopAsync();
            await tcpTunnel.StopAsync();
        } catch { }
        session.RenewCts();
        try {
            await muxConnection.ConnectAsync(session, session.Cts.Token);
            await ConnectAsync(session);
        } catch(Exception) {
            throw;
        }
    }
}
