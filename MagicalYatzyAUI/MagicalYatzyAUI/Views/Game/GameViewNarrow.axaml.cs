using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sanet.MagicalYatzy.Avalonia.Views.Game;

public partial class GameViewNarrow : GameView
{
    public GameViewNarrow()
    {
        InitializeComponent();
    }

    protected override void InitDicePanel()
    {
        base.InitDicePanel();
        DicePanelView.SetValue(Grid.ColumnProperty, 1);
        PageGrid.Children.Insert(0, DicePanelView);
    }
}