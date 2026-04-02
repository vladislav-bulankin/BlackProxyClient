using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Core.Managers.Models;
using BlackTunnel.Domain.Entities;
using BlackTunnel.Domain.Enums;
using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Managers;

public class SessionManager : ISessionManager {
    private readonly IConnectionManager connectionManager;
    private readonly IMuxConnection muxConnection;
    private SessionContext session;
    public Task EndSessionAsync (ConnectionLostReason reason) {
        throw new NotImplementedException();
    }

    public Task ResumeAsync (object ct) {
        throw new NotImplementedException();
    }

    public Task CreteSessionAsync(string connectionToken, Node node) {
        session = new() {
            Cts = new CancellationTokenSource(),
            Node = node,
            ConnectionToken = connectionToken
        };

        return Task.CompletedTask;
    }
}
