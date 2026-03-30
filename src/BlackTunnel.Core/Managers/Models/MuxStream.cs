using BlackTunnel.Domain.Enums;
using System.Threading.Channels;

namespace BlackTunnel.Core.Managers.Models; 
public class MuxStream {
    private readonly uint id;
    private readonly MuxConnection mux;
    private readonly Action<uint> onClose;
    private readonly Channel<byte[]> inbound =
        Channel.CreateUnbounded<byte[]>();

    public MuxStream (uint id, MuxConnection mux, Action<uint> onClose) {
        this.id = id;
        this.mux = mux;
        this.onClose = onClose;  
    }

    public async Task WriteAsync (byte[] data, CancellationToken ct)
        => await mux.WriteFrameAsync(
            new() {
                StreamId = id,
                Type = MuxFrameType.Data,
                Data = data
            }, ct);

    public async Task<byte[]> ReadAsync (CancellationToken ct)
        => await inbound.Reader.ReadAsync(ct);

    public async Task ReceiveDataAsync (byte[] data)
        => await inbound.Writer.WriteAsync(data);

    public async Task CloseAsync (CancellationToken ct)
        => await mux.WriteFrameAsync(
            new() {
                StreamId = id,
                Type = MuxFrameType.Close,
                Data = []
            }, ct);

    public void Close () {
        inbound.Writer.TryComplete();
        onClose(id); 
    }
}
