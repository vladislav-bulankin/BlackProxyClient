namespace BlackTunnel.Domain.Runtime; 
public class RuntimeContext {
    public string? NodeHost { get; set; }
    public int NodePort { get; set; }
    public int UdpRelayPort { get; set; }// ← заполняется после UDP ASSOCIATE
    public string? ConnectionToken;
}
