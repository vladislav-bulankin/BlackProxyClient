using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Core.Managers.Models;
using BlackTunnel.Domain.Enums;
using Microsoft.Extensions.Hosting;

namespace BlackTunnel.Core.ControlPlane; 
public class ReconnectPolicy : IHostedService {
    public byte maxRecconect { get; } = 5;
    private byte retryCount = 0;
    private TimeSpan MaxBackOff = TimeSpan.FromSeconds(5);
    private CancellationTokenSource? retryCts;
    private readonly ConnectionLostReason[] retryReason = new[] {
        ConnectionLostReason.KeepaliveTimeout,
        ConnectionLostReason.TransportError,
        ConnectionLostReason.RemoteClosed,
    };
    private readonly IConnectionHealthSink healthSink;
    private readonly ISessionManager sessionManager;
    private readonly IRouteTable routeTable;
    public ReconnectPolicy (
            IConnectionHealthSink healthSink, 
            ISessionManager sessionManager, 
            IRouteTable routeTable) {
        this.healthSink = healthSink;
        this.sessionManager = sessionManager;
        this.routeTable = routeTable;
    }

    public Task StartAsync (CancellationToken ct) {
        healthSink.StateChanged += OnStateChanged; // подписка здесь
        return Task.CompletedTask;
    }
    public Task StopAsync (CancellationToken ct) {
        healthSink.StateChanged -= OnStateChanged;
        return Task.CompletedTask;
    }

    private void OnStateChanged (ConnectionState state) {
        if (state == ConnectionState.Connected 
                || state == ConnectionState.Connecting) {
            return;
        }
        var reason = healthSink.LastDisconnectReason;
        if (retryReason.Contains(reason)) {
            _ = Task.Run( () => ReconnectAsync(reason));
        } else {
            _ = Task.Run(() => FullDisconnectAsync(reason));
        }
    }

    private async Task ReconnectAsync (ConnectionLostReason reason) {
        if(maxRecconect < retryCount) {
            await FullDisconnectAsync(reason);
            return;
        }
        retryCts?.Cancel();
        retryCts = new CancellationTokenSource();
        await ReconnectAsync(retryCts.Token);
    }

    private async Task ReconnectAsync (CancellationToken token) {
        var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
        if (delay > MaxBackOff) { delay = MaxBackOff; }
        retryCount++;
        healthSink.State = ConnectionState.Connecting;
        try {
            await Task.Delay(delay, token);
            await sessionManager.ResumeAsync(token);
            retryCount = 0;
        } catch { }
    }

    private async Task FullDisconnectAsync (ConnectionLostReason reason) {
        retryCount = 0;
        retryCts?.Cancel();
        routeTable.Clear();
            try {
            await sessionManager.EndSessionAsync(reason);
        } catch { }
    }
}
