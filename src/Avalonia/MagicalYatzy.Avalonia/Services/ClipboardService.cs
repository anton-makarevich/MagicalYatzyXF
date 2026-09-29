using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Sanet.MagicalYatzy.Services;

namespace Sanet.MagicalYatzy.Avalonia.Services;

public sealed class ClipboardService : IClipboardService
{
    public async Task SetTextAsync(string text)
    {
        var lifetime = Application.Current?.ApplicationLifetime;
        TopLevel? topLevel = lifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime singleView => TopLevel.GetTopLevel(singleView.MainView),
            _ => null
        };

        if (topLevel?.Clipboard != null)
            await topLevel.Clipboard.SetTextAsync(text);
    }
}