using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sanet.MagicalYatzy.Avalonia.Views.Fragments;

public partial class OnlineRoomsFragment : UserControl
{
    public OnlineRoomsFragment()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
