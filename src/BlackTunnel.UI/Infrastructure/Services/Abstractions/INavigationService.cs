using System.Windows.Controls;

namespace BlackTunnel.UI.Infrastructure.Services.Abstractions;

public interface INavigationService {
    void NavigateTo<TPage> () where TPage : Page;
    void NavigateTo (Page page);
    void GoBack ();
}
