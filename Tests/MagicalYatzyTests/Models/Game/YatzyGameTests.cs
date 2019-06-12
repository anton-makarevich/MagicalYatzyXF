using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Chat;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Utils;
using Xunit;

namespace MagicalYatzyTests.Models.Game
{
    public class YatzyGameTests
    {
        private YatzyGame _sut;

        public YatzyGameTests()
        {
            _sut = new YatzyGame();
        }
        
        
        private void StartGame(IPlayer player)
        {
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);
        }

        private void RollDiceToHaveValue(int requiredValue, int requiredNumberOfValues)
        {
            var numberOfValues = 0;
            if (_sut.CurrentPlayer == null)
            {
                if (_sut.Players == null || !_sut.Players.Any())
                {
                    var player = new Player(PlayerType.Local);
                    _sut.JoinGame(player);
                }

                foreach (var player in _sut.Players)
                {
                    _sut.SetPlayerReady(player,true);
                }
            }
            _sut.DiceRolled += (sender, args) => { numberOfValues = args.Value.Count(f => f == requiredValue); };
            do
            {
                Debug.Assert(_sut.CurrentPlayer != null, "_sut.CurrentPlayer != null");
                _sut.CurrentPlayer.Roll = 1;
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
                var sut = new YatzyGame(rule, Substitute.For<IDiceGenerator>());
                Assert.Equal(rule,sut.Rules.CurrentRule);
            }
        }

        [Fact]
        public void PlayersAreInitialisedWhenGameIsConstructed()
        {
            Assert.NotNull(_sut.Players);
            
            var sut = new YatzyGame(Rules.krBaby, Substitute.For<IDiceGenerator>());
            Assert.NotNull(sut.Players);
        }

        [Fact]
        public void EveryGameHasAnId()
        {
            Assert.NotNull(_sut.GameId);
            Assert.NotEmpty(_sut.GameId);
            
            var sut = new YatzyGame(Rules.krBaby, Substitute.For<IDiceGenerator>());
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
            var player = Substitute.For<IPlayer>();
            player.InGameId.Returns("0");
            StartGame(player);
            var resultAppliedCount = 0;
            RollResultEventArgs appliedResult = null;
            var result = new RollResult(Scores.SmallStraight, Rules.krSimple);
            
            _sut.ResultApplied += (sender, args) =>
            {
                resultAppliedCount++;
                appliedResult = args;
            };
            
            _sut.ApplyScore(result);
            
            Assert.Equal(1,resultAppliedCount);
            Assert.Equal(result.ScoreType,appliedResult.ScoreType);
        }

        [Fact]
        public void ApplyingScoreInvokesResultAppliedEventForResultItselfAndBonusIfResultIsNumericAndBonusCanBeApplied()
        {
            const Scores scoreToAdd = Scores.Ones;
            const Rules rule = Rules.krStandard;
            var player = Substitute.For<IPlayer>();
            player.TotalNumeric.Returns(65);
            player.MaxRemainingNumeric.Returns(40);
            player.IsReady.Returns(true);
            player.InGameId.Returns("0");
            var results = new List<RollResult>();
            foreach (var score in  EnumUtils.GetValues<Scores>())
            {
                var result = new RollResult(score, rule);
                if (score != scoreToAdd && score != Scores.Bonus)
                    result.Value = result.MaxValue;
                results.Add(result);
            }

            player.Results.Returns(results);
            StartGame(player);
            var appliedResults = new List<RollResultEventArgs>();
            var resultToAdd = new RollResult(scoreToAdd, rule);
            
            _sut.ResultApplied += (sender, args) =>
            {
                appliedResults.Add(args);
            };
            
            _sut.ApplyScore(resultToAdd);
            var bonusResult = appliedResults.LastOrDefault();
            Assert.Equal(2,appliedResults.Count);
            Assert.Equal(resultToAdd.ScoreType, appliedResults.FirstOrDefault()?.ScoreType);
            Assert.Equal(Scores.Bonus, bonusResult?.ScoreType);
            Assert.Equal(35,bonusResult?.Value);
        }
        
        [Fact]
        public void DoesNotInvokeResultAppliedEventForBonusIfBonusIsAlreadyFilled()
        {
            const Scores scoreToAdd = Scores.Ones;
            const Rules rule = Rules.krStandard;
            var player = Substitute.For<IPlayer>();
            player.IsReady.Returns(true);
            player.InGameId.Returns("0");
            var results = new List<RollResult>();
            foreach (var score in  EnumUtils.GetValues<Scores>())
            {
                var result = new RollResult(score, rule);
                if (score != scoreToAdd)
                    result.Value = result.MaxValue;
                results.Add(result);
            }

            player.Results.Returns(results);
            StartGame(player);
            var appliedResults = new List<RollResultEventArgs>();
            var resultToAdd = new RollResult(scoreToAdd, rule);
            
            _sut.ResultApplied += (sender, args) =>
            {
                appliedResults.Add(args);
            };
            
            _sut.ApplyScore(resultToAdd);
            
            Assert.Single(appliedResults);
            Assert.Equal(resultToAdd.ScoreType,appliedResults.FirstOrDefault()?.ScoreType);
        }
        
        [Fact]
        public void InvokesResultAppliedEventForBonusIfNotAllNumericAreFilledButNumericScoreIsEnoughForBonus()
        {
            const Scores scoreToAdd = Scores.Ones;
            const Rules rule = Rules.krStandard;
            _sut = new YatzyGame(rule, Substitute.For<IDiceGenerator>());
            var player = Substitute.For<IPlayer>();
            player.IsReady.Returns(true);
            player.InGameId.Returns("0");
            player.TotalNumeric.Returns(65);
            var results = new List<RollResult>();
            foreach (var score in  EnumUtils.GetValues<Scores>())
            {
                var result = new RollResult(score, rule);
                if (!result.IsNumeric && score != Scores.Bonus)
                    result.Value = result.MaxValue;
                results.Add(result);
            }

            player.Results.Returns(results);
            StartGame(player);
            var appliedResults = new List<RollResultEventArgs>();
            var resultToAdd = new RollResult(scoreToAdd, rule);
            
            _sut.ResultApplied += (sender, args) =>
            {
                appliedResults.Add(args);
            };
            
            _sut.ApplyScore(resultToAdd);
            
            var bonusResult = appliedResults.LastOrDefault();
            Assert.Equal(2,appliedResults.Count);
            Assert.Equal(resultToAdd.ScoreType,appliedResults.FirstOrDefault()?.ScoreType);
            Assert.Equal(Scores.Bonus, bonusResult?.ScoreType);
            Assert.Equal(35,bonusResult?.Value);
        }
        
        [Fact]
        public void ApplyingScoreDoesNotInvokesResultAppliedEventForBonusIfRuleDoesNotHaveBonus()
        {
            const Scores scoreToAdd = Scores.Ones;
            const Rules rule = Rules.krSimple;
            _sut = new YatzyGame(rule, Substitute.For<IDiceGenerator>());
            var player = Substitute.For<IPlayer>();
            player.IsReady.Returns(true);
            player.InGameId.Returns("0");
            var results = new List<RollResult>();
            foreach (var score in  EnumUtils.GetValues<Scores>())
            {
                var result = new RollResult(score, rule);
                if (score != scoreToAdd && score != Scores.Bonus)
                    result.Value = result.MaxValue;
                results.Add(result);
            }

            player.Results.Returns(results);
            StartGame(player);
            var appliedResults = new List<RollResultEventArgs>();
            var resultToAdd = new RollResult(scoreToAdd, rule);
            
            _sut.ResultApplied += (sender, args) =>
            {
                appliedResults.Add(args);
            };
            
            _sut.ApplyScore(resultToAdd);
            
            Assert.Single(appliedResults);
            Assert.Equal(resultToAdd.ScoreType,appliedResults.FirstOrDefault()?.ScoreType);
        }
        
        [Fact]
        public void ApplyingScoreDoesNotInvokesResultAppliedEventForBonusIfNotAllNumericAreFilled()
        {
            const Scores scoreToAdd = Scores.Ones;
            const Rules rule = Rules.krStandard;
            _sut = new YatzyGame(rule, Substitute.For<IDiceGenerator>());
            var player = Substitute.For<IPlayer>();
            player.IsReady.Returns(true);
            player.InGameId.Returns("0");
            var results = new List<RollResult>();
            foreach (var score in  EnumUtils.GetValues<Scores>())
            {
                var result = new RollResult(score, rule);
                if (!result.IsNumeric && score != Scores.Bonus)
                    result.Value = result.MaxValue;
                results.Add(result);
            }

            player.Results.Returns(results);
            StartGame(player);
            var appliedResults = new List<RollResultEventArgs>();
            var resultToAdd = new RollResult(scoreToAdd, rule);
            
            _sut.ResultApplied += (sender, args) =>
            {
                appliedResults.Add(args);
            };
            
            _sut.ApplyScore(resultToAdd);
            
            Assert.Single(appliedResults);
            Assert.Equal(resultToAdd.ScoreType,appliedResults.FirstOrDefault()?.ScoreType);
        }
        
        [Fact]
        public void ApplyingScoreAddsKniffelBonusWhenApplicable()
        {
            const Scores scoreToAdd = Scores.Ones;
            const Rules rule = Rules.krExtended;
            var diceGenerator = Substitute.For<IDiceGenerator>();
            diceGenerator.GetNextDiceResult().ReturnsForAnyArgs(1);
            _sut = new YatzyGame(rule, diceGenerator);
            var player = Substitute.For<IPlayer>();
            player.IsReady.Returns(true);
            player.InGameId.Returns("0");
            player.GetResultForScore(Scores.Kniffel).Returns(new RollResult(Scores.Kniffel, rule)
            {
                Value = 50
            });

            StartGame(player);
            _sut.ReportRoll();
            var appliedResults = new List<RollResultEventArgs>();
            var resultToAdd = Substitute.For<IRollResult>();
            resultToAdd.PossibleValue.Returns(5);
            resultToAdd.ScoreType.Returns(scoreToAdd);
            
            _sut.ResultApplied += (sender, args) =>
            {
                appliedResults.Add(args);
            };
            
            _sut.ApplyScore(resultToAdd);
            
            Assert.Single(appliedResults);
            Assert.Equal(resultToAdd.ScoreType,appliedResults.FirstOrDefault()?.ScoreType);
            Assert.True(appliedResults.FirstOrDefault()?.HasBonus);
        }
        
        [Fact]
        public void ApplyingScoreDoesNotAddKniffelBonusWhenKniffelItselfIsNotFilled()
        {
            const Scores scoreToAdd = Scores.Ones;
            const Rules rule = Rules.krExtended;
            var diceGenerator = Substitute.For<IDiceGenerator>();
            diceGenerator.GetNextDiceResult().ReturnsForAnyArgs(1);
            _sut = new YatzyGame(rule, diceGenerator);
            var player = Substitute.For<IPlayer>();
            player.IsReady.Returns(true);
            player.InGameId.Returns("0");
            player.GetResultForScore(Scores.Kniffel).Returns(new RollResult(Scores.Kniffel, rule)
            {
                Value = 0
            });

            StartGame(player);
            _sut.ReportRoll();
            var appliedResults = new List<RollResultEventArgs>();
            var resultToAdd = Substitute.For<IRollResult>();
            resultToAdd.PossibleValue.Returns(5);
            resultToAdd.ScoreType.Returns(scoreToAdd);
            
            _sut.ResultApplied += (sender, args) =>
            {
                appliedResults.Add(args);
            };
            
            _sut.ApplyScore(resultToAdd);
            
            Assert.Single(appliedResults);
            Assert.Equal(resultToAdd.ScoreType,appliedResults.FirstOrDefault()?.ScoreType);
            Assert.False(appliedResults.FirstOrDefault()?.HasBonus);
        }
        
        [Fact]
        public void ApplyingScoreDoesNotAddKniffelBonusWhenRuleDoesNotSupportExtendedBonus()
        {
            const Scores scoreToAdd = Scores.Ones;
            const Rules rule = Rules.krStandard;
            var diceGenerator = Substitute.For<IDiceGenerator>();
            diceGenerator.GetNextDiceResult().ReturnsForAnyArgs(1);
            _sut = new YatzyGame(rule, diceGenerator);
            var player = Substitute.For<IPlayer>();
            player.IsReady.Returns(true);
            player.InGameId.Returns("0");
            player.GetResultForScore(Scores.Kniffel).Returns(new RollResult(Scores.Kniffel, rule)
            {
                Value = 50
            });

            StartGame(player);
            _sut.ReportRoll();
            var appliedResults = new List<RollResultEventArgs>();
            var resultToAdd = Substitute.For<IRollResult>();
            resultToAdd.PossibleValue.Returns(5);
            resultToAdd.ScoreType.Returns(scoreToAdd);
            
            _sut.ResultApplied += (sender, args) =>
            {
                appliedResults.Add(args);
            };
            
            _sut.ApplyScore(resultToAdd);
            
            Assert.Single(appliedResults);
            Assert.Equal(resultToAdd.ScoreType,appliedResults.FirstOrDefault()?.ScoreType);
            Assert.False(appliedResults.FirstOrDefault()?.HasBonus);
        }
        
        [Fact]
        public void ApplyingScoreDoesNotAddKniffelBonusWhenRollValueIsNotKniffel()
        {
            const Scores scoreToAdd = Scores.Ones;
            const Rules rule = Rules.krExtended;
            var diceGenerator = Substitute.For<IDiceGenerator>();
            var i = 1;
            diceGenerator.GetNextDiceResult().ReturnsForAnyArgs(info =>
            {
                i++;
                return i-1;
            });
            _sut = new YatzyGame(rule, diceGenerator);
            var player = Substitute.For<IPlayer>();
            player.IsReady.Returns(true);
            player.InGameId.Returns("0");
            player.GetResultForScore(Scores.Kniffel).Returns(new RollResult(Scores.Kniffel, rule)
            {
                Value = 50
            });

            StartGame(player);
            _sut.ReportRoll();
            var appliedResults = new List<RollResultEventArgs>();
            var resultToAdd = Substitute.For<IRollResult>();
            resultToAdd.PossibleValue.Returns(5);
            resultToAdd.ScoreType.Returns(scoreToAdd);
            
            _sut.ResultApplied += (sender, args) =>
            {
                appliedResults.Add(args);
            };
            
            _sut.ApplyScore(resultToAdd);
            
            Assert.Single(appliedResults);
            Assert.Equal(resultToAdd.ScoreType,appliedResults.FirstOrDefault()?.ScoreType);
            Assert.False(appliedResults.FirstOrDefault()?.HasBonus);
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
            _sut = new YatzyGame(Rules.krMagic, new RandomDiceGenerator());
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
            _sut = new YatzyGame(Rules.krStandard, new RandomDiceGenerator());
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
            _sut = new YatzyGame(Rules.krMagic, new RandomDiceGenerator());
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
            _sut = new YatzyGame(Rules.krMagic, Substitute.For<IDiceGenerator>());
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
            _sut = new YatzyGame(Rules.krMagic, Substitute.For<IDiceGenerator>());
            
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
            _sut = new YatzyGame(Rules.krMagic, Substitute.For<IDiceGenerator>());

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
            _sut = new YatzyGame(Rules.krMagic, Substitute.For<IDiceGenerator>());

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

        [Fact]
        public void NextTurnResetsPlayersRolls()
        {
            var player1 = new Player();
            var player2 = new Player();           
            _sut.JoinGame(player1);
            _sut.JoinGame(player2);
            _sut.SetPlayerReady(player1, true);
            _sut.SetPlayerReady(player2, true);
            player1.Roll = 2;
            player2.Roll = 3;
            
            _sut.NextTurn();
            
            Assert.Equal(1,player1.Roll);
            Assert.Equal(1,player2.Roll);
        }

        [Fact]
        public void ReportRollIncrementsPlayersRollCounter()
        {
            var player1 = new Player();
            _sut.JoinGame(player1);
            _sut.SetPlayerReady(player1, true);
            
            _sut.ReportRoll();
            
            Assert.Equal(2, player1.Roll);
        }
        
        [Fact]
        public void ReportRollInvolvesDiceRolledEvent()
        {
            var player1 = new Player();
            var diceRolledInvokedTimes = 0;
            _sut.JoinGame(player1);
            _sut.SetPlayerReady(player1, true);
            _sut.DiceRolled += (sender, args) => { diceRolledInvokedTimes++; }; 
            
            player1.Roll = 3;
            
            _sut.ReportRoll();
            
            Assert.Equal(1,diceRolledInvokedTimes);
        }
        
        [Fact]
        public void ReportRollDoesNotRollDicesIfMaxAmountOfRollsIsReached()
        {
            var player1 = new Player();
            var diceRolledInvokedTimes = 0;
            _sut.JoinGame(player1);
            _sut.SetPlayerReady(player1, true);
            _sut.DiceRolled += (sender, args) => { diceRolledInvokedTimes++; }; 
            
            player1.Roll = 4;
            
            _sut.ReportRoll();
            
            Assert.Equal(0,diceRolledInvokedTimes);
        }

        [Fact]
        public void RollIsAlwaysInCorrectRange()
        {
            var player1 = new Player();
            _sut.JoinGame(player1);
            _sut.SetPlayerReady(player1, true);
            
            player1.Roll = 1;
            Assert.Equal(1, _sut.Roll);
            
            player1.Roll = 2;
            Assert.Equal(2, _sut.Roll);
            
            player1.Roll = 3;
            Assert.Equal(3, _sut.Roll);
            
            player1.Roll = -1;
            Assert.Equal(1, _sut.Roll);
            
            player1.Roll = 4;
            Assert.Equal(3, _sut.Roll);
        }
        
        [Fact]
        public void RollIsEqualToCurrentPlayersRoll()
        {
            var player1 = new Player();
            _sut.JoinGame(player1);
            _sut.SetPlayerReady(player1, true);
            
            var player2 = new Player();
            _sut.JoinGame(player2);
            _sut.SetPlayerReady(player2, true);
            
            player1.Roll = 1;
            Assert.Equal(1, _sut.Roll);
            
            player1.Roll = 2;
            Assert.Equal(2, _sut.Roll);
            
            player2.Roll = 3;
            Assert.Equal(2, _sut.Roll);
            
            player1.Roll = -1;
            Assert.Equal(1, _sut.Roll);
            
            player2.Roll = 3;
            Assert.Equal(1, _sut.Roll);
        }

        [Fact]
        public void PassingNullPlayerToChangeStyleDoesNotInvokeStyleChangedEvent()
        {
            var styleChangedTimes = 0;
            _sut.StyleChanged += (sender, args) => { styleChangedTimes++;};
            
            _sut.ChangeStyle(null, DiceStyle.Red);
            
            Assert.Equal(0,styleChangedTimes);
        }

        [Fact]
        public void NextTurnOnLastRoundInvokesGameFinishedEvent()
        {
            var gameFinishedTimes = 0;
            _sut.GameFinished += (sender, args) => { gameFinishedTimes++;};
            var player = new Player();
            _sut.JoinGame(player);

            
            _sut.NextTurn();
            
            Assert.Equal(1,gameFinishedTimes);
        }

        [Fact]
        public void ReportRollPreservesFixedValues()
        {
            var player = new Player();
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);
            _sut.ReportRoll();
            var diceToFix = _sut.LastDiceResult.DiceResults.Take(3).ToList();
            foreach (var dice in diceToFix)
            {
                _sut.FixDice(dice,true);
            }
            
            _sut.ReportRoll();

            for (var index = 0; index < 3; index++)
            {
                Assert.Equal(diceToFix[index],_sut.LastDiceResult.DiceResults[index]);
            }
        }

        [Fact]
        public void ReRollModeProducesTheSameValuesAsPreviousRoll()
        {
            var player = new Player();
            _sut = new YatzyGame(Rules.krMagic, new RandomDiceGenerator());
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);
            _sut.ReportRoll();
            var lastResults = _sut.LastDiceResult.DiceResults;
            _sut.ReRollMode = true;
            
            _sut.ReportRoll();

            for (var index = 0; index < lastResults.Count; index++)
            {
                Assert.Equal(lastResults[index],_sut.LastDiceResult.DiceResults[index]);
            }
        }
        
        [Fact]
        public void ReRollModeProducesTheSameValuesAsPreviousRollNotTheFirstRoll()
        {
            var player = new Player();
            _sut = new YatzyGame(Rules.krMagic, new RandomDiceGenerator());
            _sut.JoinGame(player);
            _sut.SetPlayerReady(player,true);
            _sut.ReportRoll();
            _sut.ReportRoll();
            var lastResults = _sut.LastDiceResult.DiceResults;
            _sut.ReRollMode = true;
            
            _sut.ReportRoll();

            for (var index = 0; index < lastResults.Count; index++)
            {
                Assert.Equal(lastResults[index],_sut.LastDiceResult.DiceResults[index]);
            }
        }
    }
}