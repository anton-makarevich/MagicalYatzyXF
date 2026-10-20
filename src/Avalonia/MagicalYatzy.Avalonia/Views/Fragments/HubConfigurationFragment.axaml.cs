using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Sanet.MagicalYatzy.Avalonia.Views.Fragments;

public partial class HubConfigurationFragment : UserControl
{
    public HubConfigurationFragment()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnEditorPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Mouse)
            e.Handled = true;
    }
}
