using BlackTunnel.Core.Abstractions.Auth;
using BlackTunnel.Core.Abstractions.ControlPlane;
using BlackTunnel.Core.Abstractions.DataPlane;
using BlackTunnel.Core.Abstractions.Managers;
using BlackTunnel.Core.Abstractions.Proxy;
using BlackTunnel.Core.Auth;
using BlackTunnel.Core.ControlPlane;
using BlackTunnel.Core.DataPlane;
using BlackTunnel.Core.Managers;
using BlackTunnel.Core.Proxy;
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
        services.AddSingleton<ITcpTunnelHandler, TcpTunnelHandler>();
        services.AddSingleton<IUdpTunnelHandler,  UdpTunnelHandler>();
        services.AddSingleton<IConnectionHealthSink,  ConnectionHealthSink>();
        services.AddSingleton<ICredentialStore, CredentialStore>();
        services.AddSingleton<IAuthController, AuthController>();
        services.AddSingleton<ISessionManager, SessionManager>();
        services.AddSingleton<IConnectionManager, ConnectionManager>();
        services.AddSingleton<IProxyService, ProxyService>();
        services.AddSingleton<IControlPlaneLoop, ControlPlaneLoop>();
        services.AddSingleton<INodeToTunPump, NodeToTunPump>();
        services.AddSingleton<ITunToNodePump, TunToNodePump>();
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
