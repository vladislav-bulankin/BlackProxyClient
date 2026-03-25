using Microsoft.Extensions.Configuration;

namespace BlackTunnel.UI.Extensions; 
public static class IConfigurationBuilderExtensions {
    public static void AddFiles(this IConfigurationBuilder builder) {
        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    }
}
