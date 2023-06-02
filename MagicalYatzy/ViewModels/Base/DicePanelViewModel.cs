using Sanet.MagicalYatzy.Models.Game;
using Sanet.MVVM.Core.ViewModels;

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
