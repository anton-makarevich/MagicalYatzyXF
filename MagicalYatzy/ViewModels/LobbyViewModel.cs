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
        private const int MaxPlayers = 4;
        
        private readonly IPlayerService _playerService;
        private bool _canAddBot = true;
        private bool _canAddHuman = true;

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
            if (!CanAddBot)
                return;
            var newBot = new Player(PlayerType.AI, Players.Select(p=>p.Name).ToList());
            AddPlayer(new PlayerViewModel(newBot));
        });

        public bool CanAddBot
        {
            get => _canAddBot;
            private set => SetProperty(ref _canAddBot, value);
        }

        public ICommand AddHumanCommand => new SimpleCommand(async () =>
        {
            if (!CanAddHuman)
                return;
            var player = await NavigationService.ShowViewModelForResultAsync<LoginViewModel,IPlayer>();
            AddPlayer(new PlayerViewModel(player));
        });

        public bool CanAddHuman
        {
            get => _canAddHuman;
            set => SetProperty(ref _canAddHuman, value);
        }

        private void AddDefaultPlayer()
        {
            if (!Players.Any())
                AddPlayer(new PlayerViewModel(_playerService.CurrentPlayer));
        }

        public override void AttachHandlers()
        {
            base.AttachHandlers();
            AddDefaultPlayer();
            CheckCanAddPlayers();
        }

        internal void AddPlayer(PlayerViewModel playerViewModel)
        {
            if (Players.Count >= MaxPlayers)
                return;
            playerViewModel.PlayerDeleted += PlayerViewModelOnPlayerDeleted;
            Players.Add(playerViewModel);
            CheckPossibilityToDeletePlayers();
            CheckCanAddPlayers();
        }

        private void PlayerViewModelOnPlayerDeleted(object sender, EventArgs e)
        {
            if (!(sender is PlayerViewModel playerVm)) return;
            playerVm.PlayerDeleted -= PlayerViewModelOnPlayerDeleted;
            Players.Remove(playerVm);
            CheckPossibilityToDeletePlayers();
            CheckCanAddPlayers();
        }

        private void CheckPossibilityToDeletePlayers()
        {
            foreach (var playerViewModel in Players)
            {
                playerViewModel.CanBeDeleted = Players.Count != 1;
            }
        }

        private void CheckCanAddPlayers()
        {
            CanAddBot = CanAddHuman = Players.Count < MaxPlayers;
        }
    }
}
