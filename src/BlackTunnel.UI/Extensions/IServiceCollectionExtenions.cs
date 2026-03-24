using BlackTunnel.Core.Abstractions.Auth;
using BlackTunnel.Core.Auth;
using BlackTunnel.Domain.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlackTunnel.UI.Extensions; 
public static class IServiceCollectionExtenions {
    public static void InjectAll (this IServiceCollection services) {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ICredentialStore, CredentialStore>();
        services.AddSingleton<IAuthController, AuthController>();
    }

    public static void ConfigurationRelations (
            this IServiceCollection services,
            HostBuilderContext context) {
        services.Configure<GeneralSettings>(context.Configuration.GetSection("GeneralSettings"));
    }
}
