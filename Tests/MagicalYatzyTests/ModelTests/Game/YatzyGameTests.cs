using System.Collections.Generic;
using System.Linq;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Utils;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class YatzyGameTests
    {
        private readonly YatzyGame _sut;

        public YatzyGameTests()
        {
            _sut = new YatzyGame();
        }
        
        
        private void StartGame()
        {
            var player = new Player();
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);
        }

        [Fact]
        public void DefaultGameRuleIsExtended()
        {
            Assert.Equal(Rules.krExtended,_sut.Rules.CurrentRule);
        }

        [Fact]
        public void GameHasRulesPassedToConstructor()
        {
            var allRules = EnumUtils.GetValues<Rules>();

            foreach (var rule in allRules)
            {
                var sut = new YatzyGame(rule);
                Assert.Equal(rule,sut.Rules.CurrentRule);
            }
        }

        [Fact]
        public void PlayersAreInitialisedWhenGameIsConstructed()
        {
            Assert.NotNull(_sut.Players);
            
            var sut = new YatzyGame(Rules.krBaby);
            Assert.NotNull(sut.Players);
        }

        [Fact]
        public void EveryGameHasAnId()
        {
            Assert.NotNull(_sut.GameId);
            Assert.NotEmpty(_sut.GameId);
            
            var sut = new YatzyGame(Rules.krBaby);
            Assert.NotNull(sut.GameId);
            Assert.NotEmpty(sut.GameId);
        }

        [Fact]
        public void FalseIsDefaultValueForIsPlaying()
        {
            Assert.False(_sut.IsPlaying);
        }

        [Fact]
        public void OneIsDefaultRollValue()
        {
            Assert.Equal(1,_sut.Roll);
        }

        [Fact]
        public void DefaultLastDiceResultHasEmptyResults()
        {
            Assert.NotNull(_sut.LastDiceResult?.DiceResults);
            Assert.Empty(_sut.LastDiceResult?.DiceResults);
        }

        [Fact]
        public void FalseIsDefaultReRollModeValue()
        {
            Assert.False(_sut.ReRollMode);
        }

        [Fact]
        public void ByDefaultThereAreNoPlayers()
        {
            Assert.Equal(0,_sut.NumberOfPlayers);
        }

        [Fact]
        public void ThereAreNoFixedDiceInitially()
        {
            Assert.Equal(0, _sut.NumberOfFixedDice);
        }
        
        [Fact]
        public void ApplyingScoreInvokesResultAppliedEventForResultOnlyIfResultIsNotNumeric()
        {
            StartGame();
            var resultAppliedCount = 0;
            RollResult appliedResult = null;
            var result = new RollResult(Scores.SmallStraight);
            
            _sut.ResultApplied += (sender, args) =>
            {
                resultAppliedCount++;
                appliedResult = args.Result as RollResult;
            };
            
            _sut.ApplyScore(result);
            
            Assert.Equal(1,resultAppliedCount);
            Assert.Equal(result,appliedResult);
        }

        [Fact]
        public void ApplyingScoreInvokesResultAppliedEventForResultItselfAndBonusIfResultIsNumeric()
        {
            StartGame();
            var resultAppliedCount = 0;
            var appliedResults = new List<RollResult>();
            var result = new RollResult(Scores.Ones);
            
            _sut.ResultApplied += (sender, args) =>
            {
                resultAppliedCount++;
                appliedResults.Add(args.Result as RollResult);
            };
            
            _sut.ApplyScore(result);
            
            Assert.Equal(2,resultAppliedCount);
            Assert.Equal(result,appliedResults.FirstOrDefault());
            Assert.Equal(Scores.Bonus, appliedResults.LastOrDefault()?.ScoreType);
        }

        [Fact]
        public void JoinGameAddsPlayerAndFiresPlayerJoinedEvent()
        {
            var player = new Player();
            Player joinedPlayer = null;
            var playerAddedCount = 0;
            _sut.PlayerJoined += (sender, args) =>
            {
                playerAddedCount++;
                joinedPlayer = args.Player as Player;
            };
            
            _sut.JoinGame(player);

            Assert.Single(_sut.Players);
            Assert.Equal(1, playerAddedCount);
            Assert.Equal(player, joinedPlayer);
        }

        [Fact]
        public void JoinGameIncreasesSeatNumberForNextPlayer()
        {
            var player1 = new Player();
            var player2 = new Player();
            
            _sut.JoinGame(player1);
            _sut.JoinGame(player2);
            
            Assert.Equal(0, player1.SeatNo);
            Assert.Equal(1, player2.SeatNo);
        }

        [Fact]
        public void SetPlayerReadyChangesPlayerIsReadyStatusAndFiresPlayerReadyEvent()
        {
            var player = new Player();
            Player readyPlayer = null;
            var playerReadyCount = 0;
            _sut.PlayerReady += (sender, args) =>
            {
                playerReadyCount++;
                readyPlayer = args.Player as Player;
            };
            
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);

            Assert.Equal(1, playerReadyCount);
            Assert.True(player.IsReady);
            Assert.Equal(player, readyPlayer);
        }

        [Fact]
        public void SettingAllPlayersReadyShouldStartGameAndFireUpdateGameEvent()
        {
            var player = new Player();
            
            var gameUpdatedCount = 0;
            _sut.GameUpdated += (sender, args) =>
            {
                gameUpdatedCount++;
            };
            
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);

            Assert.Equal(1, gameUpdatedCount);
            Assert.True(_sut.IsPlaying);
            Assert.Equal(1,_sut.Round);
        }

        [Fact]
        public void SettingAllPlayersReadyShouldActivateFirstPlayerToMakeMoveAndInvokeCorrespondingEvent()
        {
            var player1 = new Player();
            var player2 = new Player();

            Player currentPlayer = null;
            
            var turnChangedCount = 0;
            _sut.TurnChanged += (sender, args) =>
            {
                turnChangedCount++;
                currentPlayer = args.Player as Player;
            };
            
            _sut.JoinGame(player1);
            _sut.JoinGame(player2);
            _sut.SetPlayerReady(player1,true);
            _sut.SetPlayerReady(player2,true);

            Assert.Equal(1, turnChangedCount);
            Assert.NotNull(_sut.CurrentPlayer);
            Assert.Equal(player1, _sut.CurrentPlayer);
            Assert.Equal(player1, currentPlayer);
            Assert.True(_sut.CurrentPlayer.IsMyTurn);
        }

        [Fact]
        public void ChangeStyleChangesPlayerStyleAndInvokesEvent()
        {
            const DiceStyle initialStyle = DiceStyle.Classic;
            const DiceStyle nextStyle = DiceStyle.Blue;
            
            var player = new Player{ SelectedStyle = initialStyle};
            Player updatedPlayer = null;
            
            var styleChangedCount = 0;
            _sut.StyleChanged += (sender, args) =>
            {
                styleChangedCount++;
                updatedPlayer = args.Player as Player;
            };
            
            _sut.JoinGame(player);
            _sut.ChangeStyle(player, nextStyle);

            Assert.Equal(1, styleChangedCount);
            Assert.Equal(player, updatedPlayer);
            Assert.Equal(nextStyle,player.SelectedStyle);
        }
        
        [Fact]
        public void FixAllDiceFixesAllDiceOfSpecifiedValueInLastRollResultAndInvokesEventForEveryFix()
        {
            const int valueToFix = 4;
            var diceFixedCount = 0;

            var numberOfValuesToFix = 0;
            _sut.DiceRolled += (sender, args) => { numberOfValuesToFix = args.Value.Count(f => f == valueToFix); };
            do
            {
                _sut.ReportRoll();
            } while (numberOfValuesToFix<2);
            
            _sut.DiceFixed += (sender, args) =>
            {
                diceFixedCount++;
                Assert.Equal(valueToFix, args.Value);
                Assert.True(args.Isfixed);
            };
            
            _sut.FixAllDice(valueToFix,true);
            
            Assert.Equal(numberOfValuesToFix, diceFixedCount);
        }
    }
}