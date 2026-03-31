using BlackTunnel.Domain.Runtime;
using System.Net;

namespace BlackTunnel.Core.Managers.Models; 
public interface IRouteTable {
    void SaveTcpRedirect (ushort srcPort, IPEndPoint originalDest);
    bool TryGetOriginalTcpDest (ushort srcPort, out IPEndPoint dest);
    void AddOrRefreshUdpRoute (ushort srcPort, UdpRoute route);
    bool TryGetUdpRoute (ushort dstPort, out UdpRoute? route);
    void Remove (ushort remote);
    void Clear ();
}
