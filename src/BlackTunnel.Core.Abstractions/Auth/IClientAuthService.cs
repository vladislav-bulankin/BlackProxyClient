using BlackTunnel.Domain.Auth;

namespace BlackTunnel.Core.Abstractions.Auth; 
public interface IClientAuthService {
    Task<LoginResponse> LoginAsync(string username, string password);
    Task<string> OpdateAccessTokenAsync (string refreshToken);
    Task<string> GetConnectionTokenAsync (string accessToke);
    Task LogoutAsync ();
}
