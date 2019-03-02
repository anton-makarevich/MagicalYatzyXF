using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services;
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
        private IRulesService _rulesService;
        private readonly IGameService _gameService;
        private readonly ILocalizationService _localizationService;

        public LobbyViewModel(IDicePanel dicePanel, 
            IPlayerService playerService, 
            IRulesService rulesService,
            IGameService gameService,
            ILocalizationService localizationService) : base(dicePanel)
        {
            _playerService = playerService;
            _rulesService = rulesService;
            _gameService = gameService;
            _localizationService = localizationService;
        }

        public string PlayersTitle => Strings.PlayersLabel.ToUpper();

        public string RulesTitle => Strings.RulesLabel.ToUpper();
        
        public string AddBotImage => "AddBot.png";
        
        public string AddPlayerImage => "AddPlayer.png";
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
            if (player != null)
                AddPlayer(new PlayerViewModel(player));
        });

        public bool CanAddHuman
        {
            get => _canAddHuman;
            private set => SetProperty(ref _canAddHuman, value);
        }

        public ObservableCollection<RuleViewModel> Rules { get; } = new ObservableCollection<RuleViewModel>();
        public RuleViewModel SelectedRule => Rules.FirstOrDefault(r => r.IsSelected);

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
            LoadRules();
        }

        public override void DetachHandlers()
        {
            base.DetachHandlers();

            foreach (var ruleViewModel in Rules)
            {
                ruleViewModel.RuleSelected -= OnRuleSelected;
            }
            Rules.Clear();
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
            NotifyPropertyChanged(nameof(CanStartGame));
        }

        public void LoadRules()
        {
            if (Rules.Any())
                return;
            var rules = _rulesService.GetAllRules().Select(r=>new RuleViewModel(r, _rulesService, _localizationService));
            foreach (var rule in rules)
            {
                rule.RuleSelected += OnRuleSelected;
                Rules.Add(rule);
            }

            SelectRule(Models.Game.Rules.krSimple);
        }

        private void OnRuleSelected(object sender, EventArgs e)
        {
            SelectRule(((RuleViewModel)sender).Rule);
        }

        private void SelectRule(Rules rule)
        {
            foreach (var ruleViewModel in Rules)
            {
                ruleViewModel.IsSelected = false;
            }

            var ruleToSelect = Rules.FirstOrDefault(r => r.Rule == rule);
            if (ruleToSelect != null)
                ruleToSelect.IsSelected = true;
            NotifyPropertyChanged(nameof(CanStartGame));
        }

        public ICommand StartGameCommand => new SimpleCommand(async () =>
        {
            if (!CanStartGame) return;
            var rule = Rules.FirstOrDefault(f => f.IsSelected);
            if (rule==null)
                return;
            var game = await _gameService.CreateNewLocalGameAsync(rule.Rule);
            foreach (var playerViewModel in Players)
            {
                game.JoinGame(playerViewModel.Player);
            }

            await NavigationService.NavigateToViewModelAsync<GameViewModel>();
        });

        public bool CanStartGame => Rules.FirstOrDefault(f => f.IsSelected) != null && Players.Any();
    }
}
