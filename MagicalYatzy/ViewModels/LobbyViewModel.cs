using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models.Events;
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
        
        public string AddBotImage => "AddBot.png";
        public ObservableCollection<PlayerViewModel> Players { get; } = new ObservableCollection<PlayerViewModel>();
        public ICommand AddBotCommand => new SimpleCommand(() =>
        {
            var newBot = new Player(PlayerType.AI, Players.Select(p=>p.Name).ToList());
            AddPlayer(new PlayerViewModel(newBot));
        });
        private void AddDefaultPlayer()
        {
            if (!Players.Any())
                AddPlayer(new PlayerViewModel(_playerService.CurrentPlayer));
        }

        public override void AttachHandlers()
        {
            base.AttachHandlers();
            AddDefaultPlayer();
        }

        internal void AddPlayer(PlayerViewModel playerViewModel)
        {
            playerViewModel.PlayerDeleted += PlayerViewModelOnPlayerDeleted;
            Players.Add(playerViewModel);
            CheckPossibilityToDeletePlayers();
        }

        private void PlayerViewModelOnPlayerDeleted(object sender, EventArgs e)
        {
            if (!(sender is PlayerViewModel playerVm)) return;
            playerVm.PlayerDeleted -= PlayerViewModelOnPlayerDeleted;
            Players.Remove(playerVm);
            CheckPossibilityToDeletePlayers();
        }

        private void CheckPossibilityToDeletePlayers()
        {
            foreach (var playerViewModel in Players)
            {
                playerViewModel.CanBeDeleted = Players.Count != 1;
            }
        }
    }
}
