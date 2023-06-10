using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Sanet.MagicalYatzy.Avalonia.Browser.DependencyInjection;
using Sanet.MVVM.DI.Avalonia.Extensions;

[assembly: SupportedOSPlatform("browser")]

namespace Sanet.MagicalYatzy.Avalonia.Browser;

internal partial class Program
{
    private static async Task Main(string[] args) => await BuildAvaloniaApp()
        .WithInterFont()
        .UseDependencyInjection(services=>services.RegisterBrowserServices())
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}