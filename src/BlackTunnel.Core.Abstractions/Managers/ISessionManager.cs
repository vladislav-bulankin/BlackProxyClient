using BlackTunnel.Domain.Entities;
using BlackTunnel.Domain.Enums;

namespace BlackTunnel.Core.Abstractions.Managers;

public interface ISessionManager {
    Task EndSessionAsync ();
    Task ResumeAsync ();
    Task CreateSessionAsync (string connectionToken, Node node);
}
