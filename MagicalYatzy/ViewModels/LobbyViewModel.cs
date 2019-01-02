using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.Resources;

namespace Sanet.MagicalYatzy.ViewModels
{
    public class LobbyViewModel: DicePanelViewModel
    {
        public LobbyViewModel(IDicePanel dicePanel) : base(dicePanel) { }

        public string PlayersTitle => Strings.PlayersLabel.ToUpper();

        public string RulesTitle => Strings.RulesLabel.ToUpper();
    }
}
