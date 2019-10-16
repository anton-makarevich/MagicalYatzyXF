using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.Xf.Views.Base;
using Sanet.MagicalYatzy.Xf.Views.Controls.Game;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Views.Game
{
    public abstract class GameView: BaseView<GameViewModel>
    {
        protected DicePanelXf DicePanelView;
        protected override void OnAppearing()
        {
            InitDicePanel();
            base.OnAppearing();
        }

        protected virtual void InitDicePanel()
        {
            if (DicePanelView != null)
                return;

            DicePanelView = new DicePanelXf(true)
            {
                InputTransparent = Device.RuntimePlatform == Device.macOS, 
                DicePanel = ViewModel.DicePanel
            };

            ViewModel.DicePanel.DiceCount = 5;
            ViewModel.DicePanel.RollDelay = 30;
        }
    }
}