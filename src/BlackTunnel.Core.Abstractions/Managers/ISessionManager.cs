using BlackTunnel.Domain.Entities;
using BlackTunnel.Domain.Enums;

namespace BlackTunnel.Core.Abstractions.Managers;

public interface ISessionManager {
    Task EndSessionAsync (ConnectionLostReason reason);
    Task ResumeAsync (object ct);
    Task CreteSessionAsync (string connectionToken, Node node);
}
