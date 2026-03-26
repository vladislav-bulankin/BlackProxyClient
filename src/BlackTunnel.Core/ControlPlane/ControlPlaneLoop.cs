using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Runtime;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace BlackTunnel.Core.ControlPlane;

public class ControlPlaneLoop : IControlPlaneLoop {

    private readonly IConnectionHealthSink healthSink;
    private TimeSpan? currentPing;
    public TimeSpan? CurrentPing {
   
        get {
            return this.currentPing;
        }
        set {
            if (this.currentPing != value) {
                if (value.HasValue) {
                    PingTransform(value.Value);
                }
                this.currentPing = value;
            }
        }
    }
    public event Action<TimeSpan?> PingTransform;
    public ControlPlaneLoop (IConnectionHealthSink healthSink) {
        this.healthSink = healthSink;
    }

    public async Task RunAsync 
            (RuntimeContext context, CancellationToken cancellationToken) {
        var pingTask = PingAsync(context, cancellationToken);
        var keepAliveTask = KeepAliveLoop(context, cancellationToken);
        await Task.WhenAll(pingTask, keepAliveTask);
    }

    private async Task KeepAliveLoop
            (RuntimeContext context, CancellationToken cancellationToken) {
        using TcpClient tcpClient = new(context.NodeHost, context.NodePort);
        while (!cancellationToken.IsCancellationRequested) {
            try {
                await tcpClient
                    .Client
                    .SendAsync(Array.Empty<byte>(), SocketFlags.None, cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(30));
            } catch {
                healthSink
                    .OnConnectionLost(ConnectionLostReason.RemoteClosed, cancellationToken);
                break;
            }
        }
    }

    private async Task PingAsync
            (RuntimeContext context, CancellationToken cancellationToken) {
        var ping = new Ping();
        while (!cancellationToken.IsCancellationRequested) {
            try {
                var startTime = DateTime.Now;
                ping.SendAsync(context.NodeHost, cancellationToken);
                currentPing = DateTime.Now - startTime;
                await Task.Delay(TimeSpan.FromSeconds(30));
            } catch {
                break;
                /*It is possible that the provider is throttling (or dropping) ping packets */ 
            } 
        }
    }
}
