using BlackTunnel.Core.Abstractions.Auth;
using BlackTunnel.Core.Auth;
using BlackTunnel.Domain.Settings;
using BlackTunnel.UI.Infrastructure.Services;
using BlackTunnel.UI.Infrastructure.Services.Abstractions;
using BlackTunnel.UI.ViewModels;
using BlackTunnel.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlackTunnel.UI.Extensions; 
public static class IServiceCollectionExtenions {
    public static void InjectAll (this IServiceCollection services) {
        services.AddSingleton<ICredentialStore, CredentialStore>();
        services.AddSingleton<IAuthController, AuthController>();
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<ServersViewModel>();
        services.AddSingleton<LoginPage>();
        services.AddSingleton<ServersPage>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<INavigationService>(provider => {
            var mainWindow = provider.GetRequiredService<MainWindow>();
            var frame = mainWindow.MainFrame;
            return new NavigationService(frame);
        });
        
    }

    public static void ConfigurationRelations (
            this IServiceCollection services,
            HostBuilderContext context) {
        services.Configure<GeneralSettings>(context.Configuration.GetSection("GeneralSettings"));
    }
}
