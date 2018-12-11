using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.XF.Controls.Game;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Views.Base
{
    public class DicePanelView<TViewModel> : BaseView<TViewModel> where TViewModel : DicePanelViewModel
    {
        private DicePanelXF _dicePanel;
        protected void InitDicePanel()
        {
            var pageGrid = Content as Grid;
            if (pageGrid != null && ViewModel?.DicePanel != null)
            {
                _dicePanel = new DicePanelXF();
                _dicePanel.SetValue(Grid.RowSpanProperty, pageGrid.RowDefinitions.Count);
                _dicePanel.SetValue(Grid.ColumnSpanProperty, pageGrid.ColumnDefinitions.Count);
                _dicePanel.DicePanel = ViewModel.DicePanel;
                pageGrid.Children.Insert(0,_dicePanel);
                ViewModel.DicePanel.DiceCount = 5;
                ViewModel.DicePanel.RollDelay = 30;
            }
        }
        public override TViewModel ViewModel
        {
            get => base.ViewModel; 
            set
            {
                base.ViewModel = value;
                InitDicePanel();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.DicePanel.RollEnded += Roll;
            Roll();
        }

        void Roll()
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
