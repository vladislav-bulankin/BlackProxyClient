using System.Net;

namespace BlackTunnel.Domain.Runtime; 
public class UdpRoute {
    public IPEndPoint? RemoteEndpoint { get; set; }  // откуда пришёл пакет
    public IPEndPoint? LocalEndpoint { get; set; }   // куда доставить локально
    private long lastSeenTicks = DateTime.UtcNow.Ticks;

    public DateTime LastSeen
        => new DateTime(Interlocked.Read(ref lastSeenTicks), DateTimeKind.Utc);

    public void Refresh ()
        => Interlocked.Exchange(ref lastSeenTicks, DateTime.UtcNow.Ticks);
}
