using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.Avalonia.Views.Game;

public partial class GameViewWide : GameView
{
    public GameViewWide()
    {
        InitializeComponent();
    }

    protected override void InitDicePanel()
    {
        base.InitDicePanel();
        DicePanelView.SetValue(Grid.RowProperty, 1);
        DicePanelView.SetValue(Grid.ColumnProperty, 1);
        DicePanelView.SaveMargins = new Thickness(0, 0, 60, 80);
        PageGrid.Children.Insert(0, DicePanelView);
    }
}