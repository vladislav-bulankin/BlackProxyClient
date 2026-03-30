using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Abstractions.Proxy; 
public interface IProxyService {
    Task<SessionContext> ConnectAsync 
        (string host, int port, CancellationToken ct);
    Task DisconnectAsync ();
    ConnectionState State { get; }
    event EventHandler<ConnectionState> StateChanged;
}
