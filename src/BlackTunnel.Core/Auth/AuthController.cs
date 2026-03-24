using BlackTunnel.Core.Abstractions.Auth;
using BlackTunnel.Domain.Auth;

namespace BlackTunnel.Core.Auth;

public class AuthController : IAuthController {
    private readonly ICredentialStore store;
    private string acessToken;
    private string refreshToken;
    public AuthController (ICredentialStore store) {
        this.store = store;
    }
    
    public Task<(string username, string password)?> GetCredentialDataAsync () {
        throw new NotImplementedException();
    }

    public Task<LoginResponse> LoginAsync (string username, string password) {
        throw new NotImplementedException();
    }

    public async Task SetCredentialsAsync (string userName, string password) {
        await store.SaveAsync(userName, password);
    }

    public void SetTokens (string accesToken, string refreshToken) {
        this.acessToken = accesToken;
        this.refreshToken = refreshToken;
    }
}
