using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Abstractions.DataPlane; 
public interface ITcpTunnelHandler {
    void Initialize (int tcpProxyPort);
    Task StartAsync (SessionContext context, CancellationToken ct);
}
