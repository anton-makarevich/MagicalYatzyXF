using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.ViewModels
{
    public class GameResultsViewModel:BaseViewModel
    {
        private readonly IGameService _gameService;
        private readonly ILocalizationService _localizationService;
        private ObservableCollection<PlayerViewModel> _players;

        public GameResultsViewModel(
            IGameService gameService,
            ILocalizationService localizationService)
        {
            _gameService = gameService;
            _localizationService = localizationService;
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

        public string RestartImage => "PlayAgain";
        public ICommand CloseCommand => 
            new SimpleCommand(async () => await NavigationService.NavigateToRootAsync());

        public string CloseImage => "close";

        public override void AttachHandlers()
        {
            base.AttachHandlers();

            if (_gameService?.CurrentLocalGame?.Players == null
                || !_gameService.CurrentLocalGame.Players.Any())
            {
                return;
            }
            
            Players = new ObservableCollection<PlayerViewModel>(_gameService.CurrentLocalGame.Players.Select(p=>new PlayerViewModel(p, _localizationService)));
        }
    }
}