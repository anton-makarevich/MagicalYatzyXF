using System.Collections.Generic;
using System.Linq;
using Sanet.MagicalYatzy.Models.Chat;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Utils;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class YatzyGameTests
    {
        private YatzyGame _sut;

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

        private void RollDiceToHaveValue(int requiredValue, int requiredNumberOfValues)
        {
            var numberOfValues = 0;
            _sut.DiceRolled += (sender, args) => { numberOfValues = args.Value.Count(f => f == requiredValue); };
            do
            {
                _sut.ReportRoll();
            } while (numberOfValues != requiredNumberOfValues);
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
            var result = new RollResult(Scores.SmallStraight, Rules.krSimple);
            
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
            var result = new RollResult(Scores.Ones, Rules.krSimple);
            
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
                Assert.Equal(1,args.Move);
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
        public void FixAllDiceFixesOrUnfixesAllDiceOfSpecifiedValueInLastRollResultAndInvokesEventForEveryFix()
        {
            const int valueToFix = 4;
            var diceFixedCount = 0;
            var expectedValue = true;
            const int requiredNumberOfValues = 2;
            RollDiceToHaveValue(valueToFix, requiredNumberOfValues);
            
            _sut.DiceFixed += (sender, args) =>
            {
                diceFixedCount++;
                Assert.Equal(valueToFix, args.Value);
                // ReSharper disable once AccessToModifiedClosure
                Assert.Equal(expectedValue,args.Isfixed);
            };
            
            _sut.FixAllDice(valueToFix,true);
            expectedValue = false;
            _sut.FixAllDice(valueToFix,false);
            
            Assert.Equal(requiredNumberOfValues*2, diceFixedCount);
        }
        
        [Fact]
        public void FixDiceFixesOrUnfixesSingleDiceOfSpecifiedValueInLastRollResultAndInvokesEvent()
        {
            const int valueToFix = 4;
            var diceFixedCount = 0;
            var expectedValue = true;
            RollDiceToHaveValue(valueToFix, 1);

            _sut.DiceFixed += (sender, args) =>
            {
                diceFixedCount++;
                Assert.Equal(valueToFix, args.Value);
                // ReSharper disable once AccessToModifiedClosure
                Assert.Equal(expectedValue,args.Isfixed);
            };
            
            _sut.FixDice(valueToFix,true);
            Assert.True(_sut.IsDiceFixed(valueToFix));
            expectedValue = false;
            _sut.FixDice(valueToFix,false);
            
            Assert.Equal(2, diceFixedCount);
        }
        
        [Fact]
        public void ManualChangeReplacesValueWhenItIsFixed()
        {
            _sut = new YatzyGame(Rules.krMagic);
            const int valueToChange = 4;
            const int valueToChangeTo = 5;
            var diceChangedCount = 0;

            _sut.DiceChanged += (sender, args) => { diceChangedCount++; };
            
            RollDiceToHaveValue(valueToChange, 2);
            _sut.FixDice(valueToChange, true);

            var newValuesBefore = _sut.LastDiceResult.NumDiceOf(valueToChangeTo);
            _sut.ManualChange(valueToChange,valueToChangeTo, true);
            
            Assert.False(_sut.IsDiceFixed(valueToChange));
            Assert.True(_sut.IsDiceFixed(valueToChangeTo));
            Assert.Equal(1,_sut.LastDiceResult.NumDiceOf(valueToChange));
            Assert.Equal(newValuesBefore+1,_sut.LastDiceResult.NumDiceOf(valueToChangeTo));
            Assert.Equal(1,diceChangedCount);
        }
        
        [Fact]
        public void ManualChangeWorksOnlyForMagicRules()
        {
            _sut = new YatzyGame(Rules.krStandard);
            const int valueToChange = 4;
            const int valueToChangeTo = 5;
            var diceChangedCount = 0;

            _sut.DiceChanged += (sender, args) => { diceChangedCount++; };
            
            RollDiceToHaveValue(valueToChange, 2);
            _sut.FixDice(valueToChange, true);

            var newValuesBefore = _sut.LastDiceResult.NumDiceOf(valueToChangeTo);
            _sut.ManualChange(valueToChange,valueToChangeTo, true);
            
            Assert.True(_sut.IsDiceFixed(valueToChange));
            Assert.False(_sut.IsDiceFixed(valueToChangeTo));
            Assert.Equal(2,_sut.LastDiceResult.NumDiceOf(valueToChange));
            Assert.Equal(newValuesBefore,_sut.LastDiceResult.NumDiceOf(valueToChangeTo));
            Assert.Equal(0,diceChangedCount);
        }
        
        [Fact]
        public void ManualChangeReplacesValueWhenItIsNotFixed()
        {
            _sut = new YatzyGame(Rules.krMagic);
            const int valueToChange = 4;
            const int valueToChangeTo = 5;
            var diceChangedCount = 0;

            _sut.DiceChanged += (sender, args) => { diceChangedCount++; };
            
            RollDiceToHaveValue(valueToChange, 2);
            _sut.FixDice(valueToChange, true);
            
            var newValuesBefore = _sut.LastDiceResult.NumDiceOf(valueToChangeTo);
            _sut.ManualChange(valueToChange,valueToChangeTo, false);
            
            Assert.True(_sut.IsDiceFixed(valueToChange));
            Assert.False(_sut.IsDiceFixed(valueToChangeTo));
            Assert.Equal(1,_sut.LastDiceResult.NumDiceOf(valueToChange));
            Assert.Equal(newValuesBefore+1,_sut.LastDiceResult.NumDiceOf(valueToChangeTo));
            Assert.Equal(1,diceChangedCount);
        }
        
        [Fact]
        public void ManualChangeDoesNotReplacesValueWhenItIsNotInResult()
        {
            _sut = new YatzyGame(Rules.krMagic);
            const int valueToChange = 4;
            const int valueToChangeTo = 5;
            var diceChangedCount = 0;

            _sut.DiceChanged += (sender, args) => { diceChangedCount++; };
            
            RollDiceToHaveValue(valueToChange, 0);
            
            var newValuesBefore = _sut.LastDiceResult.NumDiceOf(valueToChangeTo);
            _sut.ManualChange(valueToChange,valueToChangeTo, false);
            
            Assert.Equal(0,_sut.LastDiceResult.NumDiceOf(valueToChange));
            Assert.Equal(newValuesBefore,_sut.LastDiceResult.NumDiceOf(valueToChangeTo));
            Assert.Equal(0,diceChangedCount);
        }

        [Fact]
        public void MagicalRollDoesNotSetAnyOfEmptyPokerHandsIfRulesAreNotMagical()
        {
            var player = new Player();
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);
            var magicalRollsUsedCount = 0;
            _sut.MagicRollUsed += (sender, args) => { magicalRollsUsedCount++; };
            
            _sut.ReportMagicRoll();

            var result = _sut.LastDiceResult;
            var isPokerHand = result.YatzyOfAKindScore(3) > 0 ||
                              result.YatzyOfAKindScore(4) > 0 ||
                              result.YatzyFullHouseScore() > 0 ||
                              result.YatzySmallStraightScore() > 0 ||
                              result.YatzyLargeStraightScore() > 0 ||
                              result.YatzyFiveOfAKindScore() > 0;
            
            Assert.False(isPokerHand);
            Assert.Equal(0, magicalRollsUsedCount);
        }
        
        [Fact]
        public void MagicalRollDoesNotSetAnyOfEmptyPokerHandsIfPlayerDoesNotHaveArtifacts()
        {
            _sut = new YatzyGame(Rules.krMagic);
            
            var player = new Player();
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);
            var magicalRollsUsedCount = 0;
            _sut.MagicRollUsed += (sender, args) => { magicalRollsUsedCount++; };
            
            _sut.ReportMagicRoll();

            var result = _sut.LastDiceResult;
            var isPokerHand = result.YatzyOfAKindScore(3) > 0 ||
                              result.YatzyOfAKindScore(4) > 0 ||
                              result.YatzyFullHouseScore() > 0 ||
                              result.YatzySmallStraightScore() > 0 ||
                              result.YatzyLargeStraightScore() > 0 ||
                              result.YatzyFiveOfAKindScore() > 0;
            
            Assert.False(isPokerHand);
            Assert.Equal(0, magicalRollsUsedCount);
        }
        
        [Fact]
        public void MagicalRollSetsAnyOfEmptyPokerHandsAndFiresEvent()
        {
            _sut = new YatzyGame(Rules.krMagic);

            var player = new Player
            {
                AvailableMagicalArtifacts = new List<Artifact> {new Artifact(Artifacts.MagicalRoll)}
            };
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);
            var magicalRollsUsedCount = 0;
            _sut.MagicRollUsed += (sender, args) => { magicalRollsUsedCount++; };
            
            _sut.ReportMagicRoll();

            var result = _sut.LastDiceResult;
            var isPokerHand = result.YatzyOfAKindScore(3) > 0 ||
                result.YatzyOfAKindScore(4) > 0 ||
                result.YatzyFullHouseScore() > 0 ||
                result.YatzySmallStraightScore() > 0 ||
                result.YatzyLargeStraightScore() > 0 ||
                result.YatzyFiveOfAKindScore() > 0;
            
            Assert.True(isPokerHand);
            Assert.Equal(1, magicalRollsUsedCount);
        }
        
        [Fact]
        public void MagicalRollInitializeStandartRollIfAllPokerHandsAreOccupied()
        {
            _sut = new YatzyGame(Rules.krMagic);

            var player = new Player
            {
                AvailableMagicalArtifacts = new List<Artifact> {new Artifact(Artifacts.MagicalRoll)}
            };
            
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);
            foreach (var score in Rule.PokerHands)
            {
                // ReSharper disable once PossibleNullReferenceException
                player.Results.FirstOrDefault(f => f.ScoreType == score).Value = 1;
            }
            
            var magicalRollsUsedCount = 0;
            var standardRollCount = 0;
            _sut.MagicRollUsed += (sender, args) => { magicalRollsUsedCount++; };
            _sut.DiceRolled += (sender, args) => { standardRollCount++; };
            
            _sut.ReportMagicRoll();

            Assert.Equal(0, magicalRollsUsedCount);
            Assert.Equal(1, standardRollCount);
        }

        [Fact]
        public void ResetRollSetsPlayersRollCountToOneAndReRollModeToTrue()
        {
            var player = new Player();
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player, true);
            player.Roll = 2;
            
            _sut.ResetRolls();
            Assert.True(_sut.ReRollMode);
            Assert.Equal(1,player.Roll);
        }

        [Fact]
        public void RestartGameResetsRollsForEveryPlayerAndStartsGame()
        {
            var player = new Player();
            _sut.JoinGame(player);
            player.Roll = 2;
            
            var gameUpdatedCount = 0;
            _sut.GameUpdated += (sender, args) =>
            {
                gameUpdatedCount++;
            };
            
            _sut.RestartGame();
            Assert.Equal(1,player.Roll);
            Assert.Equal(1, gameUpdatedCount);
            Assert.True(player.IsReady);
        }
        
        [Fact]
        public void RestartGameMovesPlayersAroundTable()
        {
            var player1 = new Player();
            var player2 = new Player();
            var player3 = new Player();
            _sut.JoinGame(player1);
            _sut.JoinGame(player2);
            _sut.JoinGame(player3);
            
            _sut.RestartGame();
            
            Assert.Equal(2,player1.SeatNo);
            Assert.Equal(0,player2.SeatNo);
            Assert.Equal(1,player3.SeatNo);
        }
        
        [Fact]
        public void LeavePlayerRemovesPlayerAndFiresEvent()
        {
            var player1 = new Player();
            var player2 = new Player();
            
            _sut.JoinGame(player1);
            _sut.JoinGame(player2);

            var playerLeftCount = 0;

            _sut.PlayerLeft += (sender, args) =>
            {
                playerLeftCount++;
                Assert.Equal(player2, args.Player);
            }; 
            
            _sut.LeaveGame(player2);
            
            Assert.Equal(1, playerLeftCount);
        }
        
        [Fact]
        public void LeavePlayerDoesNotStartGameIfRemainingPlayersAreNotReady()
        {
            var player1 = new Player();
            var player2 = new Player();
            
            _sut.JoinGame(player1);
            _sut.JoinGame(player2);
            
            _sut.SetPlayerReady(player2, true);
            
            var gameUpdatedCount = 0;

            _sut.GameUpdated += (sender, args) =>
            {
                gameUpdatedCount++;
            };

            _sut.LeaveGame(player2);
            
            Assert.Equal(0, gameUpdatedCount);
        }
        
        [Fact]
        public void LeavePlayerStartsGameIfRemainingPlayersAreReady()
        {
            var player1 = new Player();
            var player2 = new Player();
            
            _sut.JoinGame(player1);
            _sut.JoinGame(player2);
            
            _sut.SetPlayerReady(player1, true);
            
            var gameUpdatedCount = 0;

            _sut.GameUpdated += (sender, args) =>
            {
                gameUpdatedCount++;
            };

            _sut.LeaveGame(player2);
            
            Assert.Equal(1, gameUpdatedCount);
        }

        [Fact]
        public void SendChatMessageInvokesEvent()
        {
            var message = new ChatMessage();
            var chatMessageCalled = 0;
            _sut.ChatMessageSent += (sender, args) =>
            {
                chatMessageCalled++;
                Assert.Equal(message, args.Message);
            };
            
            _sut.SendChatMessage(message);

            Assert.Equal(1, chatMessageCalled);
        }
    }
}