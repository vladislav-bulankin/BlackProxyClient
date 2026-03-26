namespace BlackTunnel.Domain.Enums; 
public enum ConnectionLostReason {
    KeepaliveTimeout,
    TransportError,
    RemoteClosed,
    UserClosed
}
