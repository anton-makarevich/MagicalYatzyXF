using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sanet.MagicalYatzy.Avalonia.Views.Cells;

public partial class PlayerScoresCell : UserControl
{
    public PlayerScoresCell()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}