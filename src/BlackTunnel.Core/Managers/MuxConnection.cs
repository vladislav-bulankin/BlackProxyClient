using BlackTunnel.Core.Managers.Models;
using BlackTunnel.Domain;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Exceptions;
using BlackTunnel.Domain.Runtime;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace BlackTunnel.Core.Managers;

public class MuxConnection : IMuxConnection, IAsyncDisposable {

    private readonly ConcurrentDictionary<uint, MuxStream> streams = new();
    private readonly SemaphoreSlim writeLock = new(1, 1); // один писатель
    private NetworkStream? stream;
    private TcpClient? socket;
    private uint nextStreamId = 1;
    public async Task ConnectAsync (SessionContext context, CancellationToken ct) {
        var socket = new TcpClient { NoDelay = true };
        await socket.ConnectAsync(context.Node.NodeHost!, context.Node.NodePort, ct);
        stream = socket.GetStream();

        // Авторизуемся
        await AuthenticateAsync(context.ConnectionToken!, ct);
        _ = Task.Run(() => ReadLoopAsync(ct), ct);
    }

    private async Task AuthenticateAsync (string token, CancellationToken ct) {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        await WriteFrameAsync(new () {
            StreamId = 0,
            Type = MuxFrameType.Auth,
            Data = tokenBytes
        }, ct);
        var response = await ReadFrameAsync(ct);
        if (response.Type == MuxFrameType.AuthErr) {
            throw new ProxyAuthException(Encoding.UTF8.GetString(response.Data));
        }
        if (response.Type != MuxFrameType.AuthOk) {
            throw new ProxyAuthException("Неожиданный ответ при авторизации");
        }
    }

    //  Открытие нового стрима (на каждое TCP соединение приложения)
    public async Task<MuxStream> OpenStreamAsync (IPEndPoint dst, CancellationToken ct) {
        var streamId = (uint)Interlocked.Increment(ref Unsafe.As<uint, int>(ref nextStreamId));
        var stream = new MuxStream(streamId, this, id => streams.TryRemove(id, out _));
        streams[streamId] = stream;
        await WriteFrameAsync(new() {
            StreamId = streamId,
            Type = MuxFrameType.Open,
            Data = SerializeDst(dst)
        }, ct);
        return stream;
    }

    // демультиплексирует входящие фреймы 
    private async Task ReadLoopAsync (CancellationToken ct) {
        while (!ct.IsCancellationRequested) {
            var frame = await ReadFrameAsync(ct);
            if (!streams.TryGetValue(frame.StreamId, out var stream)) {
                continue;
            }
            switch (frame.Type) {
                case MuxFrameType.Data:
                await stream.ReceiveDataAsync(frame.Data);
                break;
                case MuxFrameType.Close:
                stream.Close();
                streams.TryRemove(frame.StreamId, out _);
                break;
            }
        }
    }

    // ── Запись фрейма (thread-safe) ──────────────────────────────────────────

    public async Task WriteFrameAsync (MuxFrame frame, CancellationToken ct) {
        var header = new byte[9];
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0), frame.StreamId);
        header[4] = (byte)frame.Type;
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(5), frame.Data.Length);

        await writeLock.WaitAsync(ct);
        try {
            await stream!.WriteAsync(header, ct);
            if (frame.Data.Length > 0) {
                await stream.WriteAsync(frame.Data, ct);
            }
        } finally {
            writeLock.Release();
        }
    }

    private async Task<MuxFrame> ReadFrameAsync (CancellationToken ct) {
        var header = new byte[9];
        await ReadExactAsync(header, 9, ct);
        var streamId = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(0));
        var type = (MuxFrameType)header[4];
        var length = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(5));
        var data = new byte[length];
        if (length > 0) {
            await ReadExactAsync(data, length, ct);
        }
        return new() {
            StreamId = streamId,
            Type = type,
            Data = data
        };
    }

    private byte[] SerializeDst (IPEndPoint dst) {
        // [IP(4)][PORT(2)] — 6 байт
        var ip = dst.Address.GetAddressBytes();
        var port = (ushort)dst.Port;
        var result = new byte[6];
        ip.CopyTo(result, 0);
        result[4] = (byte)(port >> 8);
        result[5] = (byte)(port & 0xFF);
        return result;
    }

    private async Task ReadExactAsync (byte[] buffer, int count, CancellationToken ct) {
        int total = 0;
        while (total < count) {
            int n = await stream!.ReadAsync(
                buffer.AsMemory(total, count - total), ct);
            if (n == 0) {
                throw new ProxyNegotiateException(
                    "Соединение разорвано при чтении");
            }
            total += n;
        }
    }

    public async ValueTask DisposeAsync () {
        foreach (var stream in streams.Values) {
            stream.Close();
        }
        streams.Clear();
        writeLock.Dispose();
        if (stream != null) {
            await stream.DisposeAsync();
        }
        socket?.Dispose();
    }
}
