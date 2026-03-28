using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Domain.Runtime;
using System.Net;

namespace BlackTunnel.Core.Managers; 
public class ConnectionManager : IConnectionManager {
	private readonly RouteTable routes = new();

	public void AddTсpRoute(ushort originalPort, (string ip, ushort port) originalDst) {
		IPEndPoint originalEndPoint = new(IPAddress.Parse(originalDst.ip), originalDst.port);
        routes.SaveTcpRedirect(originalPort, originalEndPoint);
	}

	public IPEndPoint? GetOriginalTcpDst (ushort port) {
        return routes.TryGetOriginalTcpDest(port, out var dest) ? dest : null;
    }

    public void AddUdpRoute (ushort srcPort, (string ip, ushort port) originalDst) {
        var route = new UdpRoute {
            // Куда доставить ответ — обратно приложению
            LocalEndpoint = new IPEndPoint(IPAddress.Loopback, srcPort),
            // Оригинальный dst — нужен NodeToTun чтобы подставить src в ответном пакете
            RemoteEndpoint = new IPEndPoint(IPAddress.Parse(originalDst.ip), originalDst.port)
        };
        routes.AddOrRefreshUdpRoute(srcPort, route);
    }

    public UdpRoute? GetUdpRoute (ushort dstPort) {
        return routes.TryGetUdpRoute(dstPort, out var route) ? route : null;
    }

    public void RemoveUdpRoute (ushort dstPort) 
        => routes.Remove(dstPort);
}
