using BlackTunnel.Core.Abstractions.Auth;
using KeySharp;

namespace BlackTunnel.Core.Auth; 
public class CredentialStore : ICredentialStore {
    private const string ServiceName = "AuthController";
    private const string PackageId = "BlackTunnel.UI.App";
    private const string LastUserKey = "BlackTunnelUser";
    public async Task SaveAsync (string login, string password) {
        await Task.Run(() => {
            // Сохраняем пароль по ключу = login
            Keyring.SetPassword(PackageId, ServiceName, login, password);
            // Сохраняем последний логин отдельно
            Keyring.SetPassword(PackageId, ServiceName, LastUserKey, login);
        });
    }

    public async Task<(string Login, string Password)?> GetAsync () {
        return await Task.Run<(string, string)?>(() => {
            try {
                var login = Keyring.GetPassword(PackageId, ServiceName, LastUserKey);
                if (string.IsNullOrWhiteSpace(login)) {
                    return null;
                }

                var password = Keyring.GetPassword(PackageId, ServiceName, login);
                if (string.IsNullOrWhiteSpace(password)) {
                    return null;
                }

                return (login, password);
            } catch (KeyringException ex) when (ex.Message.Contains("NotFound")) {
                // Тихо возвращаем null, если ключа нет — это нормальная ситуация
                return null;
            } catch (Exception ex) {
                // Логируем только неожиданные ошибки
                Console.WriteLine($"CredentialStore error: {ex.Message}");
                return null;
            }
        });
    }

    public async Task<bool> HasCredentialsAsync () {
        return await Task.Run(() => {
            try {
                var login = Keyring.GetPassword(PackageId, ServiceName, LastUserKey);
                return !string.IsNullOrWhiteSpace(login);
            } catch (KeyringException ex) when (ex.Message.Contains("NotFound")) {
                return false;
            } catch (Exception ex) {
                Console.WriteLine($"HasCredentials error: {ex.Message}");
                return false;
            }
        });
    }

    public async Task ClearAsync () {
        await Task.Run(() => {
            try {
                var login = Keyring.GetPassword(PackageId, ServiceName, LastUserKey);
                if (!string.IsNullOrWhiteSpace(login)) {
                    Keyring.DeletePassword(PackageId, ServiceName, login);
                }
                Keyring.DeletePassword(PackageId, ServiceName, LastUserKey);
            } catch (KeySharp.KeyringException ex) when (ex.Message.Contains("NotFound")) {
                // Ключа нет — уже очищено, ничего не делаем
            } catch (Exception ex) {
                Console.WriteLine($"ClearAsync error: {ex.Message}");
            }
        });
    }
}
