using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.ViewModels.Base
{
    public abstract class DicePanelViewModel: BaseViewModel
    {
        public IDicePanel DicePanel { get; }
        public DicePanelViewModel(IDicePanel dicePanel)
        {
            DicePanel = dicePanel;
        }
    }
}
