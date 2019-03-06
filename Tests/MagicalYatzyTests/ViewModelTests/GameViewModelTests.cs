using System.Collections.Generic;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.ViewModels;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests
{
    public class GameViewModelTests
    {
        private readonly GameViewModel _sut;
        private readonly IGameService _gameService;
        private readonly IDicePanel _dicePanel;
        private readonly IPlayer _humanPlayer;
        private readonly IPlayer _botPlayer;
        
        public GameViewModelTests()
        {
            _humanPlayer = Substitute.For<IPlayer>();
            _humanPlayer.InGameId.Returns("0");

            _botPlayer = Substitute.For<IPlayer>();
            _botPlayer.InGameId.Returns("1");
            
            _gameService = Substitute.For<IGameService>();
            _gameService.CurrentLocalGame.Players.Returns(new List<IPlayer>()
            {
                _humanPlayer,
                _botPlayer
            });
            
            _dicePanel = Substitute.For<IDicePanel>();
            _sut = new GameViewModel(_gameService, _dicePanel);
        }
        
        [Fact]
        public void HasGame()
        {
            Assert.NotNull(_sut.Game);
        }

        [Fact]
        public void HasPlayersFromGameService()
        {
            Assert.NotEmpty(_sut.Players);
            Assert.Equal(_gameService.CurrentLocalGame.Players.Count, _sut.Players.Count);
        }

        [Fact]
        public void HasCurrentPlayer()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            
            Assert.NotNull(_sut.CurrentPlayer);
        }

        [Fact]
        public void GameOnDiceFixedFixesDicePanelDiceIfCurrentPlayerIsNotHuman()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
            _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.DiceFixed += Raise.EventWith(null, new FixDiceEventArgs(_humanPlayer,2,true));
            
            _dicePanel.Received().FixDice(2,true);
        }
        
        [Fact]
        public void GameOnDiceFixedDoesNotDicePanelDiceWhenViewIsNotActive()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
            _sut.AttachHandlers();
            _sut.DetachHandlers();
            
            _gameService.CurrentLocalGame.DiceFixed += Raise.EventWith(null, new FixDiceEventArgs(_humanPlayer,2,true));
            
            _dicePanel.DidNotReceive().FixDice(2,true);
        }
    }
}