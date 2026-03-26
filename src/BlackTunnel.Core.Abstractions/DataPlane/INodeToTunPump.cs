using BlackTunnel.Domain.Runtime;
using System.Net;

namespace BlackTunnel.Core.Abstractions.DataPlane; 
public interface INodeToTunPump {
    // Слушает входящий UDP, смотрит в таблицу маршрутизации
    // и доставляет пакет нужному получателю
    Task StartAsync (RuntimeContext ctx, CancellationToken ct);
    Task StopAsync ();

    // Регистрация маршрута: src endpoint → локальный получатель
    void RegisterRoute (UdpRoute route);
    void RemoveRoute (IPEndPoint source);
}
