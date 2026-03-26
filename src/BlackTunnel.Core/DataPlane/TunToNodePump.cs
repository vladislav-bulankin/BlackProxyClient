using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.DataPlane;

public class TunToNodePump : ITunToNodePump {
    public Task StartAsync (RuntimeContext ctx, CancellationToken ct) {
        throw new NotImplementedException();
    }

    public Task StopAsync () {
        throw new NotImplementedException();
    }
}
