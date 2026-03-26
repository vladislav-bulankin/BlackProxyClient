using BlackTunnel.Core.Abstractions.Proxy;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Proxy;

public class ProxyService : IProxyService {
    public ConnectionState State => throw new NotImplementedException();

    public event EventHandler<ConnectionState> StateChanged;

    public Task<RuntimeContext> ConnectAsync (string host, int port, CancellationToken ct) {
        throw new NotImplementedException();
    }

    public Task DisconnectAsync () {
        throw new NotImplementedException();
    }
}
