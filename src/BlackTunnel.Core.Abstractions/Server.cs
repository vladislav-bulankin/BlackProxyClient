namespace BlackTunnel.Core.Abstractions; 
public class Server {
    public string? Id { get; set; }
    public string? Country { get; set; }
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? Flag { get; set; }  // эмодзи флага
    public int Ping { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }
    public bool IsPremium { get; set; } = false;
}
