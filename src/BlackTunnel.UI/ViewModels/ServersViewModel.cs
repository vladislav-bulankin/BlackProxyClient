using BlackTunnel.Core.Abstractions;
using BlackTunnel.UI.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace BlackTunnel.UI.ViewModels;

public class ServersViewModel : INotifyPropertyChanged {
    public ObservableCollection<Server> Servers { get; } = new ObservableCollection<Server>();

    private Server? selectedServer;
    private string connectionStatus = "Disconnected";
    private Brush statusColor = Brushes.Red;
    private string currentServerName = "Choose a server";
    private string buttonText = "CONNECT";
    private Brush buttonColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
    private Brush buttonHoverColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34C49E"));

    public event PropertyChangedEventHandler? PropertyChanged;

    public Brush ButtonHoverColor
    {
        get => buttonHoverColor;
        set { buttonHoverColor = value; OnPropertyChanged(); }
    }

    public Server? SelectedServer
    {
        get => selectedServer;
        set {
            selectedServer = value;
            OnPropertyChanged();
            UpdateButtonState();
            CommandManager.InvalidateRequerySuggested();
        }
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

    public string ButtonText
    {
        get => buttonText;
        set { buttonText = value; OnPropertyChanged(); }
    }

    public Brush ButtonColor
    {
        get => buttonColor;
        set { buttonColor = value; OnPropertyChanged(); }
    }

    public ICommand ConnectCommand { get; }

    public ServersViewModel () {
        ConnectCommand = new RelayCommand(ExecuteConnect, CanConnect);
        LoadFakeServers();
        SelectedServer = Servers.FirstOrDefault();
    }

    private void LoadFakeServers () {
        Servers.Add(new Server { Name = "United States - New York", City = "New York", Flag = "🇺🇸", Ping = 32 });
        Servers.Add(new Server { Name = "Germany - Frankfurt", City = "Frankfurt", Flag = "🇩🇪", Ping = 22 });
        Servers.Add(new Server { Name = "United Kingdom - London", City = "London", Flag = "🇬🇧", Ping = 65 });
        Servers.Add(new Server { Name = "Netherlands - Amsterdam", City = "Amsterdam", Flag = "🇳🇱", Ping = 18 });
        Servers.Add(new Server { Name = "Japan - Tokyo", City = "Tokyo", Flag = "🇯🇵", Ping = 148 });
    }

    // Команда активна только если сервер выбран
    private bool CanConnect () => SelectedServer != null;

    private void ExecuteConnect () {
        if (ConnectionStatus == "Connected") {
            ConnectionStatus = "Disconnected";
            StatusColor = Brushes.Red;
            ButtonText = "CONNECT";
            ButtonColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            CurrentServerName = "Choose a server";
        } else {
            if (SelectedServer == null)
                return;

            ConnectionStatus = "Connecting...";
            StatusColor = Brushes.Orange;
            ButtonText = "CANCEL";

            Task.Delay(1200).ContinueWith(_ => {
                ConnectionStatus = "Connected";
                StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
                ButtonText = "DISCONNECT";
                ButtonColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B91C1C"));
                CurrentServerName = SelectedServer.Name;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    private void UpdateButtonState () {
        if (ConnectionStatus == "Connected") {
            ButtonText = "DISCONNECT";
        }
    }

    protected void OnPropertyChanged ([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}