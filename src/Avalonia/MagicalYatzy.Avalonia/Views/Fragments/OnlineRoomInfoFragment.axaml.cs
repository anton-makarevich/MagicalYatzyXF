using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sanet.MagicalYatzy.Avalonia.Views.Fragments;

public partial class OnlineRoomInfoFragment : UserControl
{
    public OnlineRoomInfoFragment()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
