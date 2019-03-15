using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.XF.Views.Base;
using Sanet.MagicalYatzy.XF.Views.Controls.Game;

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

        protected abstract void InitDicePanel();
    }
}