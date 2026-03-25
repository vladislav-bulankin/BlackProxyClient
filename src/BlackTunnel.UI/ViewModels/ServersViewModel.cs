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

    private Server? _selectedServer;
    private string _connectionStatus = "Disconnected";
    private Brush _statusColor = Brushes.Red;
    private string _currentServerName = "Choose a server";
    private string _buttonText = "CONNECT";
    private Brush _buttonColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")); // зелёный

    public event PropertyChangedEventHandler? PropertyChanged;

    public Server? SelectedServer
    {
        get => _selectedServer;
        set { _selectedServer = value; OnPropertyChanged(); UpdateButtonState(); }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set { _connectionStatus = value; OnPropertyChanged(); }
    }

    public Brush StatusColor
    {
        get => _statusColor;
        set { _statusColor = value; OnPropertyChanged(); }
    }

    public string CurrentServerName
    {
        get => _currentServerName;
        set { _currentServerName = value; OnPropertyChanged(); }
    }

    public string ButtonText
    {
        get => _buttonText;
        set { _buttonText = value; OnPropertyChanged(); }
    }

    public Brush ButtonColor
    {
        get => _buttonColor;
        set { _buttonColor = value; OnPropertyChanged(); }
    }

    public ICommand ConnectCommand { get; }

    public ServersViewModel () {
        ConnectCommand = new RelayCommand(ExecuteConnect, CanConnect);
        LoadFakeServers();
    }

    private void LoadFakeServers () {
        Servers.Add(new Server { Name = "United States - New York", City = "New York", Flag = "🇺🇸", Ping = 32 });
        Servers.Add(new Server { Name = "Germany - Frankfurt", City = "Frankfurt", Flag = "🇩🇪", Ping = 22 });
        Servers.Add(new Server { Name = "United Kingdom - London", City = "London", Flag = "🇬🇧", Ping = 65 });
        Servers.Add(new Server { Name = "Netherlands - Amsterdam", City = "Amsterdam", Flag = "🇳🇱", Ping = 18 });
        Servers.Add(new Server { Name = "Japan - Tokyo", City = "Tokyo", Flag = "🇯🇵", Ping = 148 });
    }

    private bool CanConnect () => SelectedServer != null;

    private void ExecuteConnect () {
        if (ConnectionStatus == "Connected") {
            // Отключение
            ConnectionStatus = "Disconnected";
            StatusColor = Brushes.Red;
            ButtonText = "CONNECT";
            ButtonColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            CurrentServerName = "Choose a server";
        } else {
            // Подключение
            if (SelectedServer == null){
                return;
            }

            ConnectionStatus = "Connecting...";
            StatusColor = Brushes.Orange;
            ButtonText = "CANCEL";

            // Имитация подключения
            Task.Delay(1200).ContinueWith(_ => {
                ConnectionStatus = "Connected";
                StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
                ButtonText = "DISCONNECT";
                ButtonColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B91C1C")); // тёмно-красный
                CurrentServerName = SelectedServer.Name;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    private void UpdateButtonState () {
        if (ConnectionStatus == "Connected"){
            ButtonText = "DISCONNECT";
        }
    }

    protected void OnPropertyChanged ([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
