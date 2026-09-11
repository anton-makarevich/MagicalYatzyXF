using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sanet.MagicalYatzy.Avalonia.Views.Base;
using Sanet.MagicalYatzy.ViewModels;

namespace Sanet.MagicalYatzy.Avalonia.Views;

public partial class SettingsView : DicePanelView<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}