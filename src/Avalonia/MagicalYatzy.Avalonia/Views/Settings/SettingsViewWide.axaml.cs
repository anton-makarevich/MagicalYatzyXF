using Avalonia.Markup.Xaml;

namespace Sanet.MagicalYatzy.Avalonia.Views.Settings;

public partial class SettingsViewWide : SettingsView
{
    public SettingsViewWide()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
