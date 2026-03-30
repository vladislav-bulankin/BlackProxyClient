using BlackTunnel.Domain.Enums;

namespace BlackTunnel.Domain; 
public record MuxFrame {
    public uint StreamId { get; set; }
    public MuxFrameType Type { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}
