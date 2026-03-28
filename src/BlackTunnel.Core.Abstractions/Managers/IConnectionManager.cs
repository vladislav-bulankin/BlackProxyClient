using BlackTunnel.Domain.Runtime;
using System.Net;

namespace BlackTunnel.Core.Abstractions.Managers; 
public interface IConnectionManager {
    void AddTсpRoute (ushort originalPort, (string ip, ushort port) originalDst);
    IPEndPoint? GetOriginalTcpDst (ushort port);
    void AddUdpRoute (ushort srcPort, (string ip, ushort port) originalDst);
    UdpRoute? GetUdpRoute (ushort port);
    void RemoveUdpRoute (ushort dstPort);
}
