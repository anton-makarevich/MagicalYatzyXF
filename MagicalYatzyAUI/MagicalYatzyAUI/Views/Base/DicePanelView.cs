using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Sanet.MagicalYatzy.Avalonia.Controls.Game;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MVVM.Views.Avalonia;

namespace Sanet.MagicalYatzy.Avalonia.Views.Base
{
    public class DicePanelView<TViewModel> : BaseView<TViewModel> where TViewModel : DicePanelViewModel
    {
        private DicePanelControl? _dicePanel;
        protected void InitDicePanel()
        {
            if (_dicePanel != null)
                return;
            if (Content is not Grid pageGrid || ViewModel?.DicePanel == null) return;
            _dicePanel = new DicePanelControl(true);
            if (pageGrid.RowDefinitions.Count > 0)
                _dicePanel.SetValue(Grid.RowSpanProperty, pageGrid.RowDefinitions.Count);
            if (pageGrid.ColumnDefinitions.Count > 0)
                _dicePanel.SetValue(Grid.ColumnSpanProperty, pageGrid.ColumnDefinitions.Count);
            _dicePanel.DicePanel = ViewModel.DicePanel;
            pageGrid.Children.Insert(0, _dicePanel);
            ViewModel.DicePanel.DiceCount = 5;
            ViewModel.DicePanel.RollDelay = 30;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            InitDicePanel();
            base.OnLoaded(e);
            ViewModel.DicePanel.RollEnded += Roll;
            Roll(null,null);
        }
        
        void Roll(object sender, EventArgs e)
        {
            ViewModel.DicePanel.RollDice(null);
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            ViewModel.DicePanel.RollEnded -= Roll;
        }
    }
}
