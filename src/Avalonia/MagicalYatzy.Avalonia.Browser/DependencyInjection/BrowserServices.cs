using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.Browser.Services;
using Sanet.MagicalYatzy.Services.StorageService;

namespace Sanet.MagicalYatzy.Avalonia.Browser.DependencyInjection;

public static class BrowserServices
{
    public static void RegisterBrowserServices(this IServiceCollection services)
    {
        // Runs before CoreServices.RegisterServices, so this localStorage-backed
        // registration wins over the shared file-based default (registered with TryAdd).
        services.AddSingleton<ISettingsStorageService, LocalStorageSettingsStorageService>();
    }
}
