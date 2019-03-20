using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.XF.Views.Base;
using Sanet.MagicalYatzy.XF.Views.Controls.Game;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Views.Game
{
    public abstract class GameView: BaseView<GameViewModel>
    {
        protected DicePanelXF DicePanel;
        protected override void OnAppearing()
        {
            InitDicePanel();
            base.OnAppearing();
        }

        protected virtual void InitDicePanel()
        {
            if (DicePanel != null)
                return;

            DicePanel = new DicePanelXF
            {
                InputTransparent = Device.RuntimePlatform == Device.macOS, 
                DicePanel = ViewModel.DicePanel
            };

            ViewModel.DicePanel.DiceCount = 5;
            ViewModel.DicePanel.RollDelay = 30;
        }
    }
}