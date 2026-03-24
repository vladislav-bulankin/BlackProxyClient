using System.Windows.Controls;

namespace BlackTunnel.UI.Events; 
public class PageNavigationEventArgs : EventArgs {
    public Page TargetPage { get; }

    public PageNavigationEventArgs (Page targetPage) {
        TargetPage = targetPage ?? throw new ArgumentNullException(nameof(targetPage));
    }
}
