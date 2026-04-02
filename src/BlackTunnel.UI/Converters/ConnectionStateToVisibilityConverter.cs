using BlackTunnel.Domain.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlackTunnel.UI.Converters; 
public class ConnectionStateToVisibilityConverter : IValueConverter {
    public ConnectionState TargetState { get; set; }

    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is ConnectionState state && state == TargetState)
            return Visibility.Visible;

        return Visibility.Collapsed;
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
