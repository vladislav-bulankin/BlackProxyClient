namespace BlackTunnel.Core.Abstractions.Auth; 
public interface ICredentialStore {
    Task<(string Login, string Password)?> GetAsync ();
    Task SaveAsync (string login, string password);
    Task<bool> HasCredentialsAsync ();
    Task ClearAsync ();
}
