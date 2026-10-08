using Avalonia.Markup.Xaml;

namespace Sanet.MagicalYatzy.Avalonia.Views.Settings;

public partial class SettingsViewNarrow : SettingsView
{
    public SettingsViewNarrow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
