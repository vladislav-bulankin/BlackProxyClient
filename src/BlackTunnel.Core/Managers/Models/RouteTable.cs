using BlackTunnel.Domain.Runtime;
using System.Collections.Concurrent;
using System.Net;

namespace BlackTunnel.Core.Managers.Models;
public class RouteTable : IRouteTable {

    private readonly ConcurrentDictionary<ushort, UdpRoute> udpRoutes = new();
    private readonly ConcurrentDictionary<ushort, IPEndPoint> tcpRoutes = new();
    private readonly TimeSpan ttl;
    private readonly Timer cleanupTimer;

    public RouteTable (TimeSpan? ttl = null) {
        this.ttl = ttl ?? TimeSpan.FromSeconds(60);
        cleanupTimer = new Timer(Cleanup, null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(30));
    }

    public void SaveTcpRedirect (ushort srcPort, IPEndPoint originalDest) {
        tcpRoutes[srcPort] = originalDest;
    }

    public bool TryGetOriginalTcpDest (ushort srcPort, out IPEndPoint dest) {
        return tcpRoutes.TryRemove(srcPort, out dest); 
    }

    public void AddOrRefreshUdpRoute (ushort srcPort, UdpRoute route) {
        if (udpRoutes.TryGetValue(srcPort, out var existing)) {
            if ((DateTime.UtcNow - existing.LastSeen).TotalSeconds > 10) {
                existing.Refresh();
            }
            return;
        }
        udpRoutes.TryAdd(srcPort, route);
    }

    public bool TryGetUdpRoute (ushort dstPort, out UdpRoute? route)
        => udpRoutes.TryGetValue(dstPort, out route);

    public void Remove (ushort remote)
        => udpRoutes.TryRemove(remote, out _);

    public void Clear ()
        => udpRoutes.Clear();

    private void Cleanup (object? _) {
        var threshold = DateTime.UtcNow - ttl;
        foreach (var (key, route) in udpRoutes) {
            if (route.LastSeen < threshold) {
                udpRoutes.TryRemove(key, out var _);
            }
        }
    }

    public void Dispose () => cleanupTimer.Dispose();
}
