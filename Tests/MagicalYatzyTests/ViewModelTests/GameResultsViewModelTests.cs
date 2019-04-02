using System.Collections.Generic;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.ViewModels;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests
{
    public class GameResultsViewModelTests
    {
        private readonly IGameService _gameService = Substitute.For<IGameService>();
        private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
        private readonly IPlayer _humanPlayer;
        
        private readonly GameResultsViewModel _sut;

        public GameResultsViewModelTests()
        {
            _humanPlayer = Substitute.For<IPlayer>();
            _humanPlayer.InGameId.Returns("0");
            _humanPlayer.Roll = 1;

            _gameService = Substitute.For<IGameService>();
            _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krSimple));
            _gameService.CurrentLocalGame.Players.Returns(new List<IPlayer>()
            {
                _humanPlayer,
            });
            
            var localizationService = Substitute.For<ILocalizationService>();
            _sut = new GameResultsViewModel(_gameService, _localizationService);
        }
        
        [Fact]
        public void PopulatesListOfPlayersOnAppear()
        {
            _sut.AttachHandlers();
            
            Assert.NotNull(_sut.Players);
            Assert.NotEmpty(_sut.Players);
        }
    }
}