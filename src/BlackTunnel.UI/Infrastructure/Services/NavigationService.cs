using BlackTunnel.UI.Infrastructure.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace BlackTunnel.UI.Infrastructure.Services;

public class NavigationService : INavigationService {
    private readonly Frame frame;

    public NavigationService (Frame frame) =>
        this.frame = frame ?? throw new ArgumentNullException(nameof(frame));
    

    public void NavigateTo<TPage> () where TPage : Page {
        var page = App.Services.GetRequiredService<TPage>();
        frame.Navigate(page);
    }

    public void NavigateTo (Page page) =>
        frame.Navigate(page);

    public void GoBack () {
        if (frame.CanGoBack) {
            frame.GoBack();
        }
    }
}
