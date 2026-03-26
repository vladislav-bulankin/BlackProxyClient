using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Abstractions.ControlPlane; 
public interface IControlPlaneLoop {
    Task RunAsync(RuntimeContext context, CancellationToken cancellationToken);
}
