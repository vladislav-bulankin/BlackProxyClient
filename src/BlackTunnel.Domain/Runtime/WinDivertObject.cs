using WinDivertSharp;
namespace BlackTunnel.Domain.Runtime; 
public class WinDivertObject {
    public WinDivertBuffer buffer { get; set; } = new(65535);
    public string outboundFilter { get; set; } = """
        outbound
        and !loopback
        and ip
        and (tcp or udp)
        and !(ip.DstAddr == 127.0.0.1 and tcp.DstPort == <SERVER_PORT>)
        and not (udp and (udp.DstPort == 67 or udp.DstPort == 68))
        and not (udp and (udp.DstPort == 137 or udp.DstPort == 5355))
        and ip.DstAddr < 169.254.0.0 or ip.DstAddr >= 169.255.0.0
    """;
    public string inboundUdpFilter = """
    inbound
    and udp
    and !loopback
    and not (udp.SrcPort == 67 or udp.SrcPort == 68)
    and not (udp.SrcPort == 137 or udp.SrcPort == 5355)
    """;
    public WinDivertObject (ushort port) {
        outboundFilter
            .Replace("<SERVER_PORT>", port.ToString());
    }
}
