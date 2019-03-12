using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Media;
using Sanet.MagicalYatzy.ViewModels;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests
{
    public class GameViewModelTests
    {
        private readonly GameViewModel _sut;
        private readonly IGameService _gameService;
        private readonly IDicePanel _dicePanel;
        private readonly ISoundsProvider _soundsProvider;
        private readonly IPlayer _humanPlayer;
        private readonly IPlayer _botPlayer;

        public GameViewModelTests()
        {
            _humanPlayer = Substitute.For<IPlayer>();
            _humanPlayer.InGameId.Returns("0");

            _botPlayer = Substitute.For<IPlayer>();
            _botPlayer.InGameId.Returns("1");

            _gameService = Substitute.For<IGameService>();
            _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krSimple));
            _gameService.CurrentLocalGame.Players.Returns(new List<IPlayer>()
            {
                _humanPlayer,
                _botPlayer
            });

            _dicePanel = Substitute.For<IDicePanel>();
            _soundsProvider = Substitute.For<ISoundsProvider>();
            _sut = new GameViewModel(_gameService, _dicePanel, _soundsProvider);
        }

        [Fact]
        public void HasGame()
        {
            Assert.NotNull(_sut.Game);
        }

        [Fact]
        public void DoesNotHavePlayersFromGameServiceWhenUntilViewIsNotActive()
        {
            Assert.Empty(_sut.Players);
        }

        [Fact]
        public void ClearPlayersOnViewHiding()
        {
            _sut.AttachHandlers();
            Assert.NotEmpty(_sut.Players);
            _sut.DetachHandlers();
            Assert.Empty(_sut.Players);
        }

        [Fact]
        public void HasPlayersFromGameServiceWhenViewIsActive()
        {
            _sut.AttachHandlers();
            Assert.NotEmpty(_sut.Players);
            Assert.Equal(_gameService.CurrentLocalGame.Players.Count, _sut.Players.Count);
        }

        [Fact]
        public void HasCurrentPlayer()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();

            Assert.NotNull(_sut.CurrentPlayer);
        }

        [Fact]
        public void HasCurrentPlayerIsTrueWhenThereIsCurrentPlayer()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();

            Assert.True(_sut.HasCurrentPlayer);
        }

        [Fact]
        public void HasCurrentPlayerIsFalseWhenThereIsNoCurrentPlayer()
        {
            Assert.False(_sut.HasCurrentPlayer);
        }

        [Fact]
        public void GameOnDiceFixedFixesDicePanelDiceIfCurrentPlayerIsNotHuman()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
            _sut.AttachHandlers();

            _gameService.CurrentLocalGame.DiceFixed +=
                Raise.EventWith(null, new FixDiceEventArgs(_humanPlayer, 2, true));

            _dicePanel.Received().FixDice(2, true);
        }

        [Fact]
        public void GameOnDiceFixedDoesNotFixDicePanelDiceWhenViewIsNotActive()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
            _sut.AttachHandlers();
            _sut.DetachHandlers();

            _gameService.CurrentLocalGame.DiceFixed +=
                Raise.EventWith(null, new FixDiceEventArgs(_humanPlayer, 2, true));

            _dicePanel.DidNotReceive().FixDice(2, true);
        }

        [Fact]
        public void GameOnDiceRolledCallsRollDiceOnDicePanel()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
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
            _dicePanel.IsRolling.Returns(true);
            var results = new[] {2, 4, 6, 2, 1};
            _sut.AttachHandlers();

            _gameService.CurrentLocalGame.DiceRolled +=
                Raise.EventWith(null, new RollEventArgs(_botPlayer, results));

            _humanPlayer.ReceivedWithAnyArgs().CheckRollResults(null, null);
        }

        [Fact]
        public void GameOnPlayerLeftRemovesPlayer()
        {
            _sut.AttachHandlers();
            var initialPlayersCount = _sut.Players.Count;

            _gameService.CurrentLocalGame.PlayerLeft +=
                Raise.EventWith(null, new PlayerEventArgs(_botPlayer));

            Assert.Equal(initialPlayersCount - 1, _sut.Players.Count);
            Assert.Null(_sut.Players.FirstOrDefault(p => p.Player.InGameId == _botPlayer.InGameId));
        }

        [Fact]
        public void GameOnDiceChangedPlaysMagicSound()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            var results = new[] {2, 4, 6, 2, 1};
            _sut.AttachHandlers();

            _gameService.CurrentLocalGame.DiceChanged +=
                Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

            _soundsProvider.Received().PlaySound("magic");
        }

        [Fact]
        public void GameOnDiceChangedUpdatesCurrentPlayersResults()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            var results = new[] {2, 4, 6, 2, 1};
            _sut.AttachHandlers();

            _gameService.CurrentLocalGame.DiceChanged +=
                Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

            _humanPlayer.ReceivedWithAnyArgs().CheckRollResults(null, null);
        }

        [Fact]
        public void GameOnDiceChangedConsumesPlayersArtifact()
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            var results = new[] {2, 4, 6, 2, 1};
            _sut.AttachHandlers();

            _gameService.CurrentLocalGame.DiceChanged +=
                Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

            _humanPlayer.Received().UseArtifact(Artifacts.ManualSet);
        }

        [Fact]
        public void GameOnDiceChangedShowsResultsToSelectIfPlayerIsHuman()
        {
            _humanPlayer.IsHuman.Returns(true);
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            var results = new[] {2, 4, 6, 2, 1};
            _sut.AttachHandlers();

            _gameService.CurrentLocalGame.DiceChanged +=
                Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

            Assert.NotNull(_sut.RollResults);
        }

        [Fact]
        public void GameOnPlayerReadyUpdatePlayerIsReadyStatus()
        {
            var remoteHumanPlayer = Substitute.For<IPlayer>();
            remoteHumanPlayer.InGameId.Returns("0");
            remoteHumanPlayer.IsReady.Returns(true);
            _sut.AttachHandlers();

            _gameService.CurrentLocalGame.PlayerReady +=
                Raise.EventWith(null, new PlayerEventArgs(remoteHumanPlayer));

            Assert.True(_humanPlayer.IsReady);
        }

        [Fact]
        public void CanRollIsFalseWhenThereIsNoCurrentPlayer()
        {
            Assert.False(_sut.CanRoll);
        }

        [Fact]
        public void CanRollIsTrueWhenCurrentPlayerIsHuman()
        {
            _humanPlayer.IsHuman.Returns(true);
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();

            Assert.True(_sut.CanRoll);
        }
        
        [Fact]
        public void CanRollIsFalseWhenCurrentPlayerIsNotHuman()
        {
            _botPlayer.IsHuman.Returns(false);
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
            _sut.AttachHandlers();

            Assert.False(_sut.CanRoll);
        }

        [Fact]
        public void GameOnTurnChangedCallsUnfixAllOnDicePanel()
        {
            // Arrange
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            
            // Act
            _gameService.CurrentLocalGame.TurnChanged +=
                Raise.EventWith(null, new MoveEventArgs(_humanPlayer,1));
            
            // Assert
            _dicePanel.Received().UnfixAll();
        }
        
        [Fact]
        public void GameOnTurnChangedForceBotPlayerToMakeARoll()
        {
            // Arrange
            _botPlayer.IsBot.Returns(true);
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
            _sut.AttachHandlers();
            
            // Act
            _gameService.CurrentLocalGame.TurnChanged +=
                Raise.EventWith(null, new MoveEventArgs(_botPlayer,1));
            
            // Assert
            _gameService.CurrentLocalGame.Received().ReportRoll();
        }
        
        [Fact]
        public void GameOnTurnChangedRefreshesGameStatus()
        {
            var testAction = new Action(() =>
            {
                _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
                _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
                if (_sut.Players.Count == 0)
                    _sut.AttachHandlers();
            
                _gameService.CurrentLocalGame.TurnChanged +=
                    Raise.EventWith(null, new MoveEventArgs(_humanPlayer,1));
            });
            
            CheckIfGameStatusHasBeenRefreshed(testAction);
        }
        
        [Fact]
        public void GameOnTurnChangedDoesNotCallUnfixAllOnDicePanelIfViewIsNotActive()
        {
            // Arrange
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            _sut.DetachHandlers();
            
            // Act
            _gameService.CurrentLocalGame.TurnChanged +=
                Raise.EventWith(null, new MoveEventArgs(_humanPlayer,1));
            
            // Assert
            _dicePanel.DidNotReceive().UnfixAll();
        }

        [Fact]
        public void RollLabelIsEmptyIfThereIsNoCurrentPlayer()
        {
            Assert.Empty(_sut.RollLabel);
        }
        
        [Fact]
        public void RollLabelContainsCurrentPlayerRoll()
        {
            _botPlayer.Roll.Returns(123);
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
            
            Assert.Contains(_sut.RollLabel, _botPlayer.Roll.ToString());
        }

        [Fact]
        public void CanFixIfThereIsCurrentHumanPlayerAndItIsNotFirstRoll()
        {
            _humanPlayer.IsHuman.Returns(true);
            _humanPlayer.Roll.Returns(2);
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            
            Assert.True(_sut.CanFix);
        }
        
        [Fact]
        public void CanNotFixIfThereIsCurrentHumanPlayerAndItIsFirstRoll()
        {
            _humanPlayer.IsHuman.Returns(true);
            _humanPlayer.Roll.Returns(1);
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            
            Assert.False(_sut.CanFix);
        }
        
        [Fact]
        public void CanNotFixIfCurrentPlayerIsNotHuman()
        {
            _botPlayer.IsHuman.Returns(false);
            _botPlayer.Roll.Returns(2);
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
            _sut.AttachHandlers();
            
            Assert.False(_sut.CanFix);
        }
        
        [Fact]
        public void CanNotFixIfThereIsNoCurrentPlayer()
        {
            _sut.AttachHandlers();
            
            Assert.False(_sut.CanFix);
        }

        [Fact]
        public void TitleContainsCurrentRoundIfThereIsCurrentPlayer()
        {
            _gameService.CurrentLocalGame.Round.Returns(2);
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            
            Assert.Contains(_gameService.CurrentLocalGame.Round.ToString(),_sut.Title);
        }
        
        [Fact]
        public void TitleContainsCurrentPlayersName()
        {
            _humanPlayer.Name.Returns("Player 1");
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            
            Assert.Contains(_humanPlayer.Name,_sut.Title);
        }
        
        [Fact]
        public void TitleContainsWaitingForPlayersTextIfThereIsNoPlayer()
        {
            _sut.AttachHandlers();
            
            Assert.Contains(Strings.WaitForPlayersLabel,_sut.Title);
        }

        [Fact]
        public void MagicRollIsVisibleWhenCurrentPlayerHasCorrespondingArtifact()
        {
            _humanPlayer.MagicalArtifactsForGame.Returns(new List<Artifact>() {new Artifact(Artifacts.MagicalRoll)});
            _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            
            Assert.True(_sut.IsMagicRollVisible);
        }
        
        [Fact]
        public void MagicRollIsInvisibleWhenCurrentPlayerHasCorrespondingArtifactButRulesAreNotMagical()
        {
            _humanPlayer.MagicalArtifactsForGame.Returns(new List<Artifact>() {new Artifact(Artifacts.MagicalRoll)});
            _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krSimple));
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            
            Assert.False(_sut.IsMagicRollVisible);
        }
        
        [Fact]
        public void MagicRollIsInvisibleWhenCurrentPlayerHasCorrespondingArtifactButItsUsed()
        {
            var usedArtifact = new Artifact(Artifacts.MagicalRoll);
            usedArtifact.Use();
            _humanPlayer.MagicalArtifactsForGame.Returns(new List<Artifact>() {usedArtifact});
            _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            
            Assert.False(_sut.IsMagicRollVisible);
        }
        
        [Fact]
        public void FourthRollIsVisibleWhenCurrentPlayerHasCorrespondingArtifact()
        {
            _humanPlayer.MagicalArtifactsForGame.Returns(new List<Artifact>() {new Artifact(Artifacts.FourthRoll)});
            _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            
            Assert.True(_sut.IsFourthRollVisible);
        }

        [Fact]
        public void ManualSetIsVisibleWhenCurrentPlayerHasCorrespondingArtifact()
        {
            _humanPlayer.MagicalArtifactsForGame.Returns(new List<Artifact>() {new Artifact(Artifacts.ManualSet)});
            _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _sut.AttachHandlers();
            
            Assert.True(_sut.IsManualSetVisible);
        }

        private void CheckIfGameStatusHasBeenRefreshed(Action testAction)
        {
            var currentPlayerUpdated = 0;
            var canRollUpdatedTimes = 0;
            var canFixUpdatedTimes = 0;
            var titleUpdatedTimes = 0;
            var playerResultsRefreshedTimes = 0;

            var magicRollVisibilityCheckedTimes = 0;
            var fourthRollVisibilityCheckedTimes = 0;
            var manualSetRollVisibilityCheckedTimes = 0;
            
            _sut.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(_sut.CurrentPlayer):
                        currentPlayerUpdated++;
                        break;
                    case nameof(_sut.CanRoll):
                        canRollUpdatedTimes++;
                        break;
                    case nameof(_sut.CanFix):
                        canFixUpdatedTimes++;
                        break;
                    case nameof(_sut.Title):
                        titleUpdatedTimes++;
                        break;
                    case nameof(_sut.IsMagicRollVisible):
                        magicRollVisibilityCheckedTimes++;
                        break;
                    case nameof(_sut.IsManualSetVisible):
                        manualSetRollVisibilityCheckedTimes++;
                        break;
                    case nameof(_sut.IsFourthRollVisible):
                        fourthRollVisibilityCheckedTimes++;
                        break;
                }
            };
            _sut.AttachHandlers();
            
            foreach (var playerViewModel in _sut.Players)
            {
                playerViewModel.PropertyChanged += (sender, args) =>
                {
                    switch (args.PropertyName)
                    {
                        case nameof(playerViewModel.Results):
                            playerResultsRefreshedTimes++;
                            break;
                    }
                };
            }

            testAction();
            
            Assert.Equal(1,currentPlayerUpdated);
            Assert.Equal(1,canRollUpdatedTimes);
            Assert.Equal(1,canFixUpdatedTimes);
            Assert.Equal(1, titleUpdatedTimes);
            
            if (_sut.HasCurrentPlayer)
                Assert.Equal(_sut.Players.Count, playerResultsRefreshedTimes);

            if (_sut.Game.Rules.CurrentRule != Rules.krMagic) return;
            Assert.Equal(1,magicRollVisibilityCheckedTimes);
            Assert.Equal(1,fourthRollVisibilityCheckedTimes);
            Assert.Equal(1, manualSetRollVisibilityCheckedTimes);
        }
    }
}