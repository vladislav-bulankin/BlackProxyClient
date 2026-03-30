using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Abstractions.ControlPlane; 
public interface IControlPlaneLoop {
    Task RunAsync(SessionContext context, CancellationToken cancellationToken);
}
