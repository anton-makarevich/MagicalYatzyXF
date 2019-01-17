using System.Collections.ObjectModel;
using System.Linq;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.ViewModels
{
    public class LobbyViewModel: DicePanelViewModel
    {
        private readonly IPlayerService _playerService;
        public LobbyViewModel(IDicePanel dicePanel, IPlayerService playerService) : base(dicePanel)
        {
            _playerService = playerService;
        }

        public string PlayersTitle => Strings.PlayersLabel.ToUpper();

        public string RulesTitle => Strings.RulesLabel.ToUpper();
        public ObservableCollection<PlayerViewModel> Players { get; } = new ObservableCollection<PlayerViewModel>();

        private void AddDefaultPlayer()
        {
            if (!Players.Any())
                Players.Add(new PlayerViewModel(_playerService.CurrentPlayer));
        }

        public override void AttachHandlers()
        {
            base.AttachHandlers();
            AddDefaultPlayer();
        }
    }
}
