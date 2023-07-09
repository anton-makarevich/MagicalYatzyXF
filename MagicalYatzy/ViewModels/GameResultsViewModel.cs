using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Services.Api;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MagicalYatzy.ViewModels
{
    public class GameResultsViewModel:BaseViewModel
    {
        private readonly IGameService _gameService;
        private readonly ILocalizationService _localizationService;
        private readonly IApiClient _apiClient;
        private ObservableCollection<PlayerViewModel> _players;

        public GameResultsViewModel(
            IGameService gameService,
            ILocalizationService localizationService,
            IApiClient apiClient)
        {
            _gameService = gameService;
            _localizationService = localizationService;
            _apiClient = apiClient;
        }

        public ObservableCollection<PlayerViewModel> Players
        {
            get => _players;
            set => SetProperty(ref _players, value);
        }

        public ICommand RestartGameCommand => new SimpleCommand((async () =>
        {
            var players = _gameService.CurrentLocalGame.Players;
            var rule = _gameService.CurrentLocalGame.Rules.CurrentRule;
            var game = await _gameService.CreateNewLocalGameAsync(rule);
            foreach (var player in players)
            {
                game.JoinGame(player);
            }
            await NavigationService.NavigateToViewModelAsync<GameViewModel>();
        }));

        public string RestartImage => "PlayAgain.png";
        public string CloseImage => "Close.png";

        public ICommand CloseCommand => 
            new SimpleCommand(async () => await NavigationService.NavigateToRootAsync());


        public string CloseButtonContent => _localizationService.GetLocalizedString("CloseButtonContent"); 
        public string Title => _localizationService.GetLocalizedString("GameFinishedLabel");
        public string AgainLabel => _localizationService.GetLocalizedString("AgainLabel");

        public override void AttachHandlers()
        {
            base.AttachHandlers();

            if (_gameService?.CurrentLocalGame?.Players == null
                || !_gameService.CurrentLocalGame.Players.Any())
            {
                return;
            }
            
            Players = new ObservableCollection<PlayerViewModel>(_gameService.CurrentLocalGame.Players.Select(p=>new PlayerViewModel(p, _localizationService)));
#pragma warning disable 4014
            SaveScoreAsync();
#pragma warning restore 4014
        }

        private async Task SaveScoreAsync()                                           
        {
            foreach (var player in Players.Where(p => p.Player.IsHuman))
            {
                await _apiClient.SaveScoreAsync(
                    player.Name,
                    player.Total,
                    _gameService.CurrentLocalGame.Rules.CurrentRule);
            }
        }
    }
}