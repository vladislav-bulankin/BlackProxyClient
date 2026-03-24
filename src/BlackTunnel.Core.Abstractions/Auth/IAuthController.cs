using BlackTunnel.Domain.Auth;

namespace BlackTunnel.Core.Abstractions.Auth; 
public interface IAuthController {
    Task<LoginResponse> LoginAsync (string username, string password);
    Task<(string username, string password)?> GetCredentialDataAsync ();
    void SetTokens (string accesToken, string refreshToken);
    Task SetCredentialsAsync (string userName, string password);
}
