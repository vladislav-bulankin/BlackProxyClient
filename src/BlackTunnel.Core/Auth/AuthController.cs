using BlackTunnel.Core.Abstractions.Auth;
using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Domain.Auth;
using BlackTunnel.Domain.Entities;
using System.Xml.Linq;

namespace BlackTunnel.Core.Auth;

public class AuthController : IAuthController {
    private readonly ICredentialStore store;
    private readonly ISessionManager sessionManager;
    private string acessToken;
    private string refreshToken;
    public AuthController (
            ICredentialStore store,
            ISessionManager sessionManager) {
        this.store = store;
        this.sessionManager = sessionManager;
    }

    public async Task DisConnectasync () {

    }

    public async Task<AuthBaseResponse> ConnectAsync (string nodeHost, int nodePotr) {
        try {
            var node = new Node() {
                NodeHost = nodeHost,
                NodePort = nodePotr,
            };
            var connToken = await GetConnectionTokenAsync(node);
            if (connToken is null || !connToken.IsSuccess) {
                return new AuthBaseResponse {
                    IsSuccess = false,
                    Message = connToken?.Message ?? "Failed to retrieve the connection token"
                };
            }
            await sessionManager.CreteSessionAsync(connToken.ConnectionToken!, node);
            return new AuthBaseResponse { IsSuccess = true };
        } catch (OperationCanceledException) {
            return new AuthBaseResponse { 
                IsSuccess = false, 
                Message = "Connection attempt was canceled" };
        } catch (Exception) {
            return new AuthBaseResponse {
                IsSuccess = false,
                Message = "A network error occurred while establishing the connection"
            };
        }
    }

    public Task<(string username, string password)?> GetCredentialDataAsync () {
        throw new NotImplementedException();
    }

    public async Task<LoginResponse> LoginAsync (string username, string password) {
        var result = new LoginResponse() {
            IsSuccess = true,
            AccessToken = Guid.NewGuid().ToString(),
            RefreshToken = Guid.NewGuid().ToString()
        };
        if (result.IsSuccess) {
            await SetCredentialsAsync(username, password);
        }
        return result;
    }

    

    public async Task<AuthBaseResponse> UpdateAccessTokenAsync () {
        if (string.IsNullOrWhiteSpace(refreshToken)) {
            return new(false, "not authorieze");
        }
        var result = new LoginResponse() {
            IsSuccess = true,
            AccessToken = Guid.NewGuid().ToString(),
            RefreshToken = Guid.NewGuid().ToString()
        };
        if (!result.IsSuccess) {
            return new(result.IsSuccess, result.Message);
        }
        acessToken = result.AccessToken;
        refreshToken = result.RefreshToken;
        return new() { IsSuccess = true };
    }
    private async Task<ConnTokenResponse> GetConnectionTokenAsync (Node node) {
        if (string.IsNullOrWhiteSpace(acessToken)) {
            return new(false, "not authorieze");
        }
        return new(true, string.Empty) {
            ConnectionToken = "some big token data"
        };
    }
    private async Task SetCredentialsAsync (string userName, string password) {
        await store.SaveAsync(userName, password);
    }
}
