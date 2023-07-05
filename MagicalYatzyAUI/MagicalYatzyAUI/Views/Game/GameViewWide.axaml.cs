﻿using System;
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
        PageGrid.Children.Insert(0, DicePanelView);
    }
    
    public void RollResultSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e?.ItemData is RollResultViewModel viewModel)
            ViewModel?.ApplyRollResult(viewModel.RollResult);
    }
}