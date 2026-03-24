using BlackTunnel.UI.Views;
using System.Windows;
using System.Windows.Input;

namespace BlackTunnel.UI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow () {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage());
        }

        private void Window_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}