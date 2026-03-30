using BlackTunnel.Domain;
using BlackTunnel.Domain.Runtime;
using System.Net;

namespace BlackTunnel.Core.Managers.Models; 
public interface IMuxConnection {
    Task ConnectAsync (RuntimeContext ctx, CancellationToken ct);
    Task<MuxStream> OpenStreamAsync (IPEndPoint dst, CancellationToken ct);
    Task WriteFrameAsync (MuxFrame frame, CancellationToken ct);
}
