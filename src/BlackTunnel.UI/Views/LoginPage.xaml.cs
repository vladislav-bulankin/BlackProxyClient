using BlackTunnel.UI.Events;
using BlackTunnel.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BlackTunnel.UI.Views {
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page {
        public LoginPage (LoginViewModel viewModel) {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OnNavigationRequested (object? sender, PageNavigationEventArgs e) {
            NavigationService?.Navigate(e.TargetPage);
        }

        private void OnUnloaded (object sender, RoutedEventArgs e) {
            if (DataContext is LoginViewModel vm) {
                vm.NavigationRequested -= OnNavigationRequested;
            }
            this.Unloaded -= OnUnloaded;   // отписываемся
        }

        private void PasswordBox_PasswordChanged (object sender, RoutedEventArgs e) {
            if (DataContext is LoginViewModel vm) {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}
