using BlackTunnel.UI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace BlackTunnel.UI; 
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    private IHost? host;

    protected override void OnStartup (StartupEventArgs e) {
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
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit (ExitEventArgs e) {
        using (host) {
            await host.StopAsync();
        }
        base.OnExit(e);
    }
}
