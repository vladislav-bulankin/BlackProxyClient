using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Abstractions.DataPlane; 
public interface ITunToNodePump {
    Task StartAsync (RuntimeContext ctx, CancellationToken ct);
    Task StopAsync ();
}
