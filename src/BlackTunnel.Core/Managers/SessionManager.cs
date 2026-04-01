using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Domain.Enums;

namespace BlackTunnel.Core.Managers;

public class SessionManager : ISessionManager {
    public Task EndSessionAsync (ConnectionLostReason reason) {
        throw new NotImplementedException();
    }

    public Task ResumeAsync (object ct) {
        throw new NotImplementedException();
    }
}
