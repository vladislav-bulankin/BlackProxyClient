using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Domain.Enums;

namespace BlackTunnel.Core.ControlPlane;

public class ConnectionHealthSink : IConnectionHealthSink {

    private ConnectionState state = ConnectionState.Connected;
    private ConnectionLostReason lastDisconnectReason;

    public ConnectionLostReason LastDisconnectReason {
        get { return lastDisconnectReason; }
        set {
            if (lastDisconnectReason == value) { return; }
            lastDisconnectReason = value;
        }
    }
    public ConnectionState State
    {
        get { return state; }
        set {
            if(state == value) {  return; }
            state = value;
            StateChanged?.Invoke(value);
        }
    }

    public event Action<ConnectionState>? StateChanged;

    public void OnConnectionLost 
            (ConnectionLostReason reason, CancellationToken ct) {
        if (State == ConnectionState.Connected || State == ConnectionState.Connecting) { return; }
        LastDisconnectReason = reason;
        State = ConnectionState.Disconnected;
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.Cancel();
        switch (reason) {
            case ConnectionLostReason.KeepaliveTimeout:
            case ConnectionLostReason.TransportError:
                State = ConnectionState.Disconnected; 
            break;
            case ConnectionLostReason.RemoteClosed:
            case ConnectionLostReason.UserClosed:
            case ConnectionLostReason.AuthFailed:
            case ConnectionLostReason.NegotiationFailed:
                State = ConnectionState.Error;
            break;
        }
        
    }
}
