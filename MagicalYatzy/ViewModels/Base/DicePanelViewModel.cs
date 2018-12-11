using Sanet.MagicalYatzy.Models.Game;
using System;
using System.Collections.Generic;
using System.Text;

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
