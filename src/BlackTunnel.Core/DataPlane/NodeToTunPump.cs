using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Domain.Runtime;
using System.Net;

namespace BlackTunnel.Core.DataPlane;

public class NodeToTunPump : INodeToTunPump {
    public void RegisterRoute (UdpRoute route) {
        throw new NotImplementedException();
    }

    public void RemoveRoute (IPEndPoint source) {
        throw new NotImplementedException();
    }

    public Task StartAsync (RuntimeContext ctx, CancellationToken ct) {
        throw new NotImplementedException();
    }

    public Task StopAsync () {
        throw new NotImplementedException();
    }
}
