using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Abstractions.Managers; 
public interface IConnectionManager {
    Task ConnectAsync(SessionContext context);
    Task ReconnectAsync ();
    Task DisconnectAsync ();
}
