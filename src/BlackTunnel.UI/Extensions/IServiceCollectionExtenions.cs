using BlackTunnel.Core.Abstractions.Auth;
using BlackTunnel.Core.Auth;
using BlackTunnel.Domain.Settings;
using BlackTunnel.UI.ViewModels;
using BlackTunnel.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlackTunnel.UI.Extensions; 
public static class IServiceCollectionExtenions {
    public static void InjectAll (this IServiceCollection services) {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ICredentialStore, CredentialStore>();
        services.AddSingleton<IAuthController, AuthController>();
        services.AddTransient<LoginViewModel>();    
        services.AddTransient<ServersViewModel>();
        services.AddTransient<LoginPage>();
        services.AddTransient<ServersPage>();
    }

    public static void ConfigurationRelations (
            this IServiceCollection services,
            HostBuilderContext context) {
        services.Configure<GeneralSettings>(context.Configuration.GetSection("GeneralSettings"));
    }
}
