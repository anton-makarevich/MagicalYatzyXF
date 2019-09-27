using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Api;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Navigation;
using Sanet.MagicalYatzy.ViewModels;
using Xunit;

namespace MagicalYatzyTests.ViewModels
{
    public class GameResultsViewModelTests
    {
        private readonly IGameService _gameService = Substitute.For<IGameService>();
        private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
        private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
        private readonly IApiClient _apiClient = Substitute.For<IApiClient>();
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
            _sut = new GameResultsViewModel(_gameService, _localizationService, _apiClient);
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

        [Fact]
        public void RestartImageHasCorrectValue()
        {
            Assert.Equal("PlayAgain.png" ,_sut.RestartImage);
        }

        [Fact]
        public async Task CloseShouldNavigateToTheRoot()
        {
            _sut.CloseCommand.Execute(null);

            await _navigationService.Received().NavigateToRootAsync();
        }

        [Fact]
        public void CloseImageHasCorrectValue()
        {
            Assert.Equal("close.png", _sut.CloseImage);
        }

        [Fact]
        public void DoesNotCreatePlayersCollectionIfThereIsNoPlayersInGame()
        {
            _game.Players.Returns(new List<IPlayer>());
            _sut.AttachHandlers();
            
            Assert.Null(_sut.Players);
        }

        [Fact]
        public async Task SavesScoreForEveryPlayerOnAppear()
        {
            var players = new List<IPlayer>()
            {
                Substitute.For<IPlayer>(),
                Substitute.For<IPlayer>()
            };
            _game.Players.Returns(players);
            _sut.AttachHandlers();
            await Task.Delay(50);

            Assert.Equal(
                players.Count, 
                _apiClient.ReceivedCalls().Count());
        }
    }
}