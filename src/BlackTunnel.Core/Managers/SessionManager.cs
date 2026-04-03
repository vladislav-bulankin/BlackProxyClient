using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Core.Managers.Models;
using BlackTunnel.Domain.Entities;
using BlackTunnel.Domain.Runtime;

namespace BlackTunnel.Core.Managers;

public class SessionManager : ISessionManager {
    private readonly IConnectionManager connectionManager;
    private readonly IMuxConnection muxConnection;
    private SessionContext? session;

    public SessionManager (
            IConnectionManager connectionManager, 
            IMuxConnection muxConnection) {
        this.connectionManager = connectionManager;
        this.muxConnection = muxConnection;
    }

    public async Task EndSessionAsync () {
        if (session is null) { return; }
        try {
            await connectionManager.DisconnectAsync(session);
        } catch { //ignore
        } finally {
            session = null;
        }
    }

    public async Task ResumeAsync () {
        try {
            await connectionManager.ReconnectAsync(session);
        } catch (Exception) { throw; }
    }

    public async Task CreateSessionAsync(string connectionToken, Node node) {
        session = new() {
            Cts = new CancellationTokenSource(),
            Node = node,
            ConnectionToken = connectionToken
        };
        try {
            await muxConnection.ConnectAsync(session, session.Cts.Token);
            await connectionManager.ConnectAsync(session);
        } catch (Exception) {
            throw;
        }
    }
}
