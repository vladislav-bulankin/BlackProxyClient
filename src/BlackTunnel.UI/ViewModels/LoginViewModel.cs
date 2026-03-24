using BlackTunnel.Core.Abstractions.Auth;
using BlackTunnel.Domain.Auth;
using BlackTunnel.UI.Commands;
using BlackTunnel.UI.Events;
using BlackTunnel.UI.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BlackTunnel.UI.ViewModels; 
public class LoginViewModel : INotifyPropertyChanged {
    private readonly IAuthController authController;

    public LoginViewModel (IAuthController authController) {
        this.authController = authController;
        LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
        Task.Run(async () => await LoadSavedCredentials());
    }

    private string? username;
    private string? password;
    private string? errorMessage;
    private bool isLoading;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? Username
    {
        get => username;
        set { username = value; OnPropertyChanged(); }
    }

    public string? Password
    {
        get => password;
        set { password = value; OnPropertyChanged(); }
    }

    public string? ErrorMessage
    {
        get => errorMessage;
        set { errorMessage = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => isLoading;
        set { isLoading = value; OnPropertyChanged(); }
    }

    public ICommand LoginCommand { get; }

    public event EventHandler<PageNavigationEventArgs>? NavigationRequested;

    private async Task LoadSavedCredentials () {
        var credData = await authController.GetCredentialDataAsync();
        if(!credData.HasValue) { return; }
        Username = credData.Value.username; 
        Password = credData.Value.password;
        await LoginAsync();
    }

    private bool CanLogin () =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !IsLoading;

    private async Task LoginAsync () {
        IsLoading = true;
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)) {
            return;
        }
        var response = await authController.LoginAsync(Username ?? "", Password ?? "");

        await ProcessingLoginRequest(response);
    }

    protected void OnPropertyChanged ([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async Task ProcessingLoginRequest(LoginResponse response) {
        if (response is not null && response.IsSuccess 
                && !string.IsNullOrEmpty(response.AccessToken)) {
            await authController.SetCredentialsAsync(Username, Password);
            authController.SetTokens(response.AccessToken!, response.RefreshToken!);
            NavigationRequested?.Invoke(this, new PageNavigationEventArgs(new ServersPage()));
        } else {
            ErrorMessage = response.Message ?? "Login failed. Please try again.";
        }

        IsLoading = false;
    }
}
