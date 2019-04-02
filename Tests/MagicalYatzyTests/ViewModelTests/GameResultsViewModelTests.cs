using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Navigation;
using Sanet.MagicalYatzy.ViewModels;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests
{
    public class GameResultsViewModelTests
    {
        private readonly IGameService _gameService = Substitute.For<IGameService>();
        private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
        private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
        private readonly IPlayer _humanPlayer;
        private readonly IGame _game = Substitute.For<IGame>();
        
        private readonly GameResultsViewModel _sut;

        public GameResultsViewModelTests()
        {
            _humanPlayer = Substitute.For<IPlayer>();
            _humanPlayer.InGameId.Returns("0");
            _humanPlayer.Roll = 1;

            _gameService.CurrentLocalGame.Returns(_game);
            _game.Rules.Returns(new Rule(Rules.krSimple));
            _game.Players.Returns(new List<IPlayer>()
            {
                _humanPlayer,
            });
            
            Substitute.For<ILocalizationService>();
            _sut = new GameResultsViewModel(_gameService, _localizationService);
            _sut.SetNavigationService(_navigationService);
        }
        
        [Fact]
        public void PopulatesListOfPlayersOnAppear()
        {
            _sut.AttachHandlers();
            
            Assert.NotNull(_sut.Players);
            Assert.NotEmpty(_sut.Players);
        }

        [Fact]
        public async Task RestartGameCreatesNewCurrentGameWithTheSamePlayers()
        {
            var newGame = Substitute.For<IGame>();
            _gameService.CreateNewLocalGameAsync(Arg.Any<Rules>()).Returns(Task.FromResult(newGame));
            
            _sut.RestartGameCommand.Execute(null);

            await _gameService.Received().CreateNewLocalGameAsync(Rules.krSimple);
            newGame.Received().JoinGame(_humanPlayer);
        }
        
        [Fact]
        public async Task RestartGameNavigatesToGameView()
        {
            _sut.RestartGameCommand.Execute(null);

            await _navigationService.Received().NavigateToViewModelAsync<GameViewModel>();
        }
    }
}