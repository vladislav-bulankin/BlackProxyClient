using BlackTunnel.Core.Abstractions;
using BlackTunnel.UI.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BlackTunnel.UI.ViewModels; 
public class ServersViewModel : INotifyPropertyChanged {
    public ObservableCollection<Server> Servers { get; } = new ObservableCollection<Server>();

    private Server? selectedServer;
    private string connectionStatus = "Disconnected";
    private Brush statusColor = Brushes.Red;
    private string currentServerName = "Choose a server";

    public event PropertyChangedEventHandler? PropertyChanged;

    public Server? SelectedServer
    {
        get => selectedServer;
        set { selectedServer = value; OnPropertyChanged(); UpdateCurrentServer(); }
    }

    public string ConnectionStatus
    {
        get => connectionStatus;
        set { connectionStatus = value; OnPropertyChanged(); }
    }

    public Brush StatusColor
    {
        get => statusColor;
        set { statusColor = value; OnPropertyChanged(); }
    }

    public string CurrentServerName
    {
        get => currentServerName;
        set { currentServerName = value; OnPropertyChanged(); }
    }

    public ICommand ConnectCommand { get; }

    public ServersViewModel () {
        ConnectCommand = new RelayCommand(Connect, CanConnect);

        LoadFakeServers();   // пока тестовые данные
    }

    private void LoadFakeServers () {
        Servers.Add(new Server { Name = "United States - New York", City = "New York", Flag = "🇺🇸", Ping = 32 });
        Servers.Add(new Server { Name = "United Kingdom", City = "London", Flag = "🇬🇧", Ping = 68 });
        Servers.Add(new Server { Name = "Germany", City = "Frankfurt", Flag = "🇩🇪", Ping = 25 });
        Servers.Add(new Server { Name = "Japan", City = "Tokyo", Flag = "🇯🇵", Ping = 145 });
        Servers.Add(new Server { Name = "Netherlands", City = "Amsterdam", Flag = "🇳🇱", Ping = 18 });
    }

    private bool CanConnect () => SelectedServer != null;

    private void Connect () {
        if (SelectedServer == null)
            return;

        ConnectionStatus = "Connecting...";
        StatusColor = Brushes.Orange;

        // Имитация подключения
        Task.Delay(1500).ContinueWith(_ => {
            ConnectionStatus = "Connected";
            StatusColor = Brushes.LimeGreen;
            CurrentServerName = SelectedServer.Name;
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void UpdateCurrentServer () {
        CurrentServerName = SelectedServer?.Name ?? "Choose a server";
    }

    protected void OnPropertyChanged ([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
