using BlackTunnel.Domain.Auth;

namespace BlackTunnel.Core.Abstractions.Auth; 
public interface IAuthController {
    Task<LoginResponse> LoginAsync (string username, string password);
    Task<(string username, string password)?> GetCredentialDataAsync ();
    Task<AuthBaseResponse> UpdateAccessTokenAsync ();
    Task<AuthBaseResponse> ConnectAsync (string nodeHost, int nodePotr);
    Task DisConnectasync ();
}
