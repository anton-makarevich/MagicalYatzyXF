using System.Collections.Generic;
using System.Linq;
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
        
        [Fact]
        public void GameOnDiceRolledCallsRollDiceOnDicePanel()
        {
            _dicePanel.IsRolling.Returns(false, true);
            var results = new[] {2, 4, 6, 2, 1};
            _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.DiceRolled += 
                Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

            _dicePanel.Received(1).RollDice(Arg.Any<List<int>>());
        }
        
        [Fact]
        public void GameOnDiceRolledDoesNotCallRollDiceOnDicePanelWhenViewIsNotActive()
        {
            _dicePanel.IsRolling.Returns(false, true);
            var results = new[] {2, 4, 6, 2, 1};
            _sut.AttachHandlers();
            _sut.DetachHandlers();
            _gameService.CurrentLocalGame.DiceRolled += 
                Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

            _dicePanel.DidNotReceive().RollDice(Arg.Any<List<int>>());
        }
        
        [Fact]
        public void GameOnDiceRollCallsCheckRollResultsForCurrentPlayer()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _dicePanel.IsRolling.Returns( true);
            var results = new[] {2, 4, 6, 2, 1};
            _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.DiceRolled += 
                Raise.EventWith(null, new RollEventArgs(_botPlayer, results));

            _humanPlayer.ReceivedWithAnyArgs().CheckRollResults(null,null);
        }

        [Fact]
        public void GameOnPlayerLeftRemovesPlayer()
        {
            var initialPlayersCount = _sut.Players.Count;
            _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.PlayerLeft += 
                Raise.EventWith(null, new PlayerEventArgs(_botPlayer));

            Assert.Equal(initialPlayersCount - 1, _sut.Players.Count);
            Assert.Null(_sut.Players.FirstOrDefault(p => p.Player.InGameId == _botPlayer.InGameId));
        }
    }
}