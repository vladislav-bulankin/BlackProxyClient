using BlackTunnel.Domain.Entities;
using BlackTunnel.Domain.Enums;

namespace BlackTunnel.Domain.Runtime; 
public class SessionContext {
    public int UdpRelayPort { get; set; }// ← заполняется после UDP ASSOCIATE
    public TaskCompletionSource<int> UdpPortReady { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    public string? ConnectionToken { get; set; }
    public ConnectionState ConState { get; set; }
    public Node? Node { get; set; }
    public DateTime ConnectAt { get; set; }
    public DateTime ExpirationAt { get; set; }
    public CancellationTokenSource? Cts { get; set; }
}
