using BlackTunnel.Domain.Enums;

namespace BlackTunnel.Core.Abstractions.ControlPlane; 
public interface IConnectionHealthSink {
    public event Action<ConnectionState>? StateChanged;
    public ConnectionState State { get; }
    void OnConnectionLost (ConnectionLostReason reason, CancellationToken ct);
}
