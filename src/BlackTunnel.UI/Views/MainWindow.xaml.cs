using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BlackTunnel.UI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public Frame MainFrame => MainFrameElement;
        public MainWindow () {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }

        private void MinimizeBtn_Click (object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void CloseBtn_Click (object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();
    }
}