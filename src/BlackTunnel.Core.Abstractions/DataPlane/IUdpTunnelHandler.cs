using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Abstractions.DataPlane; 
public interface IUdpTunnelHandler {
    void Initialize (int udpProxyPort);
    Task StartAsync (SessionContext context, CancellationToken ct);
    Task StopAsync ();
}
