using System;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.Xf.Views.Controls.Game;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Views.Base
{
    public class DicePanelView<TViewModel> : BaseView<TViewModel> where TViewModel : DicePanelViewModel
    {
        private DicePanelXf _dicePanel;
        protected void InitDicePanel()
        {
            if (_dicePanel != null)
                return;
            if (Content is Grid pageGrid && ViewModel?.DicePanel != null)
            {
                _dicePanel = new DicePanelXf
                {
                    InputTransparent = Device.RuntimePlatform == Device.macOS
                };
                if (pageGrid.RowDefinitions.Count > 0)
                    _dicePanel.SetValue(Grid.RowSpanProperty, pageGrid.RowDefinitions.Count);
                if (pageGrid.ColumnDefinitions.Count > 0)
                    _dicePanel.SetValue(Grid.ColumnSpanProperty, pageGrid.ColumnDefinitions.Count);
                _dicePanel.DicePanel = ViewModel.DicePanel;
                pageGrid.Children.Insert(0, _dicePanel);
                ViewModel.DicePanel.DiceCount = 5;
                ViewModel.DicePanel.RollDelay = 30;
            }
        }

        protected override void OnAppearing()
        {
            InitDicePanel();
            base.OnAppearing();
            ViewModel.DicePanel.RollEnded += Roll;
            Roll(null,null);
        }

        void Roll(object sender, EventArgs e)
        {
            ViewModel.DicePanel.RollDice(null);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ViewModel.DicePanel.RollEnded -= Roll;
        }
    }
}
