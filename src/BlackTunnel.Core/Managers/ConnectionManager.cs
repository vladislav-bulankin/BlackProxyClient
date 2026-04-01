using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Core.Abstractions.Managers;
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
    private readonly IConnectionHealthSink healthSink;
    private readonly GeneralSettings settings;
    private volatile SessionContext session;
    public ConnectionManager (
            ITunToNodePump tunToNode, 
            INodeToTunPump nodeToTun, 
            ITcpTunnelHandler tcpTunnel, 
            IUdpTunnelHandler udpTunnel,
            IConnectionHealthSink healthSink,
            IOptions<GeneralSettings> options) {
        this.tunToNode = tunToNode;
        this.nodeToTun = nodeToTun;
        this.tcpTunnel = tcpTunnel;
        this.udpTunnel = udpTunnel;
        this.healthSink = healthSink;
        this.settings = options.Value;
    }

    public async Task ConnectAsync (SessionContext context) {
        if(context is null) { 
            healthSink
                .OnConnectionLost(
                    ConnectionLostReason.RemoteClosed, 
                    CancellationToken.None); 
            return; 
        }
        await tunToNode.StartAsync(context?.Cts?.Token ?? CancellationToken.None);
        await nodeToTun.StartAsync(context, context?.Cts?.Token ?? CancellationToken.None);
        tcpTunnel.Initialize(settings.TcpProxyPort);
        await tcpTunnel.StartAsync(context, context.Cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(2));
        udpTunnel.Initialize(settings.UdpProxyPort);
        await udpTunnel.StartAsync(context, context.Cts.Token);
        session = context;
    }

    public async Task DisconnectAsync () {
        await tunToNode.StopAsync();
        await nodeToTun.StopAsync();
        await udpTunnel.StopAsync();
        await tcpTunnel.StopAsync();
        if(session is not null) {
            healthSink
            .OnConnectionLost(ConnectionLostReason.UserClosed, session.Cts.Token);
            session.Cts.Cancel();
            session = null;
        }
    }

    public async Task ReconnectAsync () {
        try {
            await tunToNode.StopAsync();
            await nodeToTun.StopAsync();
            await udpTunnel.StopAsync();
            await tcpTunnel.StopAsync();
        } catch {
            healthSink
                .OnConnectionLost(ConnectionLostReason.TransportError, session.Cts.Token);
        }
        try {
            
            await ConnectAsync(session);
        } catch {
            healthSink
                .OnConnectionLost(ConnectionLostReason.TransportError, session.Cts.Token);
        }
    }
}
