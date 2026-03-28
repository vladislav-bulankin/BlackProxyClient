namespace BlackTunnel.Core.Abstractions.DataPlane; 
public interface ITunToNodePump {
    Task StartAsync (CancellationToken ct);
    Task StopAsync ();
}
