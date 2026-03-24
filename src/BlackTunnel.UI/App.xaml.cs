using BlackTunnel.UI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace BlackTunnel.UI; 
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    private readonly IHost host;
    public App () {
        host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) => {
                builder.AddFiles();
            })
            .ConfigureServices((context, services) => {
                services.InjectAll();
                services.ConfigurationRelations(context);
            })
            .Build();
    }

    protected override async void OnStartup (StartupEventArgs e) {
        await host.StartAsync();
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit (ExitEventArgs e) {
        using (host) {
            await host.StopAsync();
        }
        base.OnExit(e);
    }
}
