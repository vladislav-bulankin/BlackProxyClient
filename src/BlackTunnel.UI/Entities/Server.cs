namespace BlackTunnel.Core.Abstractions; 
public class Server {
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? Flag { get; set; }  // эмодзи флага
    public int Ping { get; set; }
    public string? Host { get; private set; }
    public int Port { get; private set; }
}
