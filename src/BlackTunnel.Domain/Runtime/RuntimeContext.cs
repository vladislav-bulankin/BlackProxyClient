using BlackTunnel.Domain.Enums;

namespace BlackTunnel.Domain.Runtime; 
public class RuntimeContext {
    public Guid SessionId { get; set; }
    public int NodeId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? NodeHost { get; set; }
    public int NodePort { get; set; }
    public string? SessionKey { get; set; }
    public ConnectionState State { get; set; }   
    public DateTime ConnectedAt { get; set; }
    public CancellationTokenSource? Cts { get; set; }
    public RouteTable Routes { get; } = new RouteTable(TimeSpan.FromSeconds(60));
}
