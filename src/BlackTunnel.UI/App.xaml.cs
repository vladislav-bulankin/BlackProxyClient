using BlackTunnel.UI.Extensions;
using BlackTunnel.UI.Infrastructure.Services.Abstractions;
using BlackTunnel.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices.JavaScript;
using System.Windows;

namespace BlackTunnel.UI; 
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    private IHost? host;

    protected override async void OnStartup (StartupEventArgs e) {
        base.OnStartup (e);
        host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) => {
                builder.AddFiles();
            })
            .ConfigureServices((context, services) => {
                services.InjectAll();
                services.ConfigurationRelations(context);
            })
            .Build();
        await host.StartAsync();
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
        var navigationService = host.Services.GetRequiredService<INavigationService>();
        navigationService.NavigateTo<LoginPage>();
    }

    protected override async void OnExit (ExitEventArgs e) {
        using (host) {
            await host.StopAsync();
        }
        base.OnExit(e);
    }
    public static IServiceProvider Services =>
        ((App)Current)?.host?.Services ?? throw new InvalidOperationException("ServiceProvider not initialized");
}
