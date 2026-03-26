using System.Collections.Concurrent;
using System.Net;

namespace BlackTunnel.Domain.Runtime; 
public class RouteTable {

    private readonly ConcurrentDictionary<IPEndPoint, UdpRoute> routes = new();
    private readonly TimeSpan ttl;
    private readonly Timer cleanupTimer;

    public RouteTable (TimeSpan? ttl = null) {
        this.ttl = ttl ?? TimeSpan.FromSeconds(60);
        cleanupTimer = new Timer(Cleanup, null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(30));
    }

    public void AddOrRefresh (UdpRoute route) {
        if(routes.TryGetValue(route.RemoteEndpoint, out var existing)){
            if ((DateTime.UtcNow - existing.LastSeen).TotalSeconds > 10) {
                existing.Refresh();
            }
            return;
        }
        // Новый маршрут — добавляем
        routes.TryAdd(route.RemoteEndpoint, route);
    }

    public bool TryGet (IPEndPoint remote, out UdpRoute? route)
        => routes.TryGetValue(remote, out route);

    public void Remove (IPEndPoint remote)
        => routes.TryRemove(remote, out _);

    public void Clear ()
        => routes.Clear();

    private void Cleanup (object? _) {
        var threshold = DateTime.UtcNow - ttl;
        foreach (var (key, route) in routes) {
            if (route.LastSeen < threshold) {
                routes.TryRemove(key, out var _);
            }
        }
    }

    public void Dispose () => cleanupTimer.Dispose();
}
