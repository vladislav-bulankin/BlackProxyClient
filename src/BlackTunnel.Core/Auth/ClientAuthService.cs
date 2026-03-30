using BlackTunnel.Core.Abstractions.Auth;
using BlackTunnel.Domain.Auth;

namespace BlackTunnel.Core.Auth;

public class ClientAuthService : IClientAuthService {

    /// <summary>
    /// получение подписанного connection токена по access токену
    /// </summary>
    /// <param name="accessToke"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<string> GetConnectionTokenAsync (string accessToke) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// получение токенов access && refresh по паре email password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<LoginResponse> LoginAsync (string username, string password) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// легальное отключение от узла 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task LogoutAsync () {
        throw new NotImplementedException();
    }

    /// <summary>
    /// получение access токена по refresh токену
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<string> OpdateAccessTokenAsync (string refreshToken) {
        throw new NotImplementedException();
    }
}
