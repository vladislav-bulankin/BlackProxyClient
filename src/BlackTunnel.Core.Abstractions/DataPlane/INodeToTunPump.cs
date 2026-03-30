using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Abstractions.DataPlane; 
public interface INodeToTunPump {
    // Слушает входящий UDP, смотрит в таблицу маршрутизации
    // и доставляет пакет нужному получателю
    Task StartAsync (SessionContext ctx, CancellationToken ct);
    Task StopAsync ();
}
