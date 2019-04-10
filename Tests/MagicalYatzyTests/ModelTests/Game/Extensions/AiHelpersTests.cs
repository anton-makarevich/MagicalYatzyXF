using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Sanet.MagicalYatzy.Utils;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game.Extensions
{
    public class AiHelpersTests
    {
        #region ResultHelpers
        [Fact]
        public void NumPairsReturnsCorrectAmountOfPairs()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 2, 1, 2, 1}};
            var result = sut.NumPairs();
            Assert.Equal(2,result);
            
            sut = new DieResult {DiceResults = new List<int> {1, 3, 1, 2, 1}};
            result = sut.NumPairs();
            Assert.Equal(1,result);
            
            sut = new DieResult {DiceResults = new List<int> {1, 3, 6, 2, 5}};
            result = sut.NumPairs();
            Assert.Equal(0,result);
        }

        [Fact]
        public void XInRowReturnsFirstValueAndAmountOfValuesInRow()
        {
            var sut = new DieResult {DiceResults = new List<int> {3, 2, 4, 2, 4}};
            var (firstValue, numberOfValuesInRow) = sut.XInRow();
            Assert.Equal(2,firstValue);
            Assert.Equal(3,numberOfValuesInRow);
            
            sut = new DieResult {DiceResults = new List<int> {3, 2, 4, 2, 1}};
            (firstValue, numberOfValuesInRow) = sut.XInRow();
            Assert.Equal(1,firstValue);
            Assert.Equal(4,numberOfValuesInRow);
            
            sut = new DieResult {DiceResults = new List<int> {3, 5, 4, 2, 6}};
            (firstValue, numberOfValuesInRow) = sut.XInRow();
            Assert.Equal(2,firstValue);
            Assert.Equal(5,numberOfValuesInRow);
        }

        [Fact]
        public void MinAllowedValueIsMoreThanZeroForEveryScoreExceptBonus()
        {
            var rule = new Rule(Rules.krSimple);
            foreach (var score in rule.ScoresForRule)
            {
                var result = new RollResult(score, rule.CurrentRule);
                
                Assert.True(result.MinAllowableValue()>0);
            }
        }
        #endregion

        #region RollAgain
        [Fact]
        public void BotDoesNotNeedToRollAgainIfGotImportantScore()
        {
            var importantScores = new[]
            {
                Scores.Kniffel,
                Scores.LargeStraight,
                Scores.SmallStraight,
                Scores.FullHouse
            };

            foreach (var score in importantScores)
            {
                var botPlayer = GetBotPlayerWithMaxResultFor(score);

                Assert.False(botPlayer.AiNeedsToRollAgain());   
            }
        }
        
        [Fact]
        public void BotNeedsToRollAgainIfDidNotGetImportantScore()
        {
            var importantScores = new[]
            {
                Scores.Kniffel,
                Scores.LargeStraight,
                Scores.SmallStraight,
                Scores.FullHouse
            };

            var notImportantScores = EnumUtils.GetValues<Scores>()
                .Where(s => !importantScores.Contains(s)).ToList();

            foreach (var score in notImportantScores)
            {
                var botPlayer = GetBotPlayerWithMaxResultFor(score);
                foreach (var importantScore in importantScores)
                {
                    botPlayer.GetResultForScore(importantScore).ReturnsNull();
                }
                
                Assert.True(botPlayer.AiNeedsToRollAgain());   
            }
        }
        #endregion
        
        #region FixDiceLogic
        [Fact]
        public void WhenBotPlayerGotThreeDiceOfValueFiveHeFixesThem()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){5,5,5,2,3}};
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = GetBotPlayerWithResultForScore(Scores.Fives, 15);
            
            botPlayer.AiFixDice(game);
            
            game.Received().FixAllDice(5,true);
        }
        
        [Fact]
        public void WhenBotPlayerGotThreeDiceOfValueSixHeFixesThem()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){6,6,6,2,3}};
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = GetBotPlayerWithResultForScore(Scores.Sixs, 18);
            
            botPlayer.AiFixDice(game);
            
            game.Received().FixAllDice(6,true);
        }
        
        [Fact]
        public void WhenBotPlayerGotThreeDiceInRowAndNeedsLargeStraightHeFixesThem()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){1,6,6,2,3}};
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = GetBotPlayerWithResultForScore(Scores.LargeStraight, 0);
            
            botPlayer.AiFixDice(game);
            
            game.Received().FixDice(1,true);
            game.Received().FixDice(2,true);
            game.Received().FixDice(3,true);
        }
        
        [Fact]
        public void WhenBotPlayerGotTwoPairsAndNeedsFullHouseHeFixesThem()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){6,6,2,2,3}};
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = GetBotPlayerWithResultForScore(Scores.FullHouse, 0);
            SetValueForScore(botPlayer, Scores.Fours);
            
            botPlayer.AiFixDice(game);
            
            game.Received().FixAllDice(6,true);
            game.Received().FixAllDice(2,true);
        }

        [Fact]
        public void IfBotGotTwoSameDiceAndDoesNotHaveKniffelThenHeFixesThem()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){3,4,1,3,3}};
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = GetBotPlayerWithResultForScore(Scores.Kniffel, 0);
            SetValueForScore(botPlayer, Scores.LargeStraight);
            SetValueForScore(botPlayer, Scores.Threes);

            botPlayer.AiFixDice(game);
            
            game.Received().FixAllDice(3,true);
        }
        
        [Fact]
        public void IfBotGotTwoSameDiceAndDoesNotHaveThreeOfKindThenHeFixesThem()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){3,4,1,3,3}};
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = GetBotPlayerWithResultForScore(Scores.Kniffel, 0);
            SetValueForScore(botPlayer, Scores.LargeStraight);
            SetValueForScore(botPlayer, Scores.Kniffel);
            SetValueForScore(botPlayer, Scores.Threes);

            botPlayer.AiFixDice(game);
            
            game.Received().FixAllDice(3,true);
        }
        
        [Fact]
        public void IfBotGotTwoSameDiceAndDoesNotHaveFourOfKindThenHeFixesThem()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){3,4,1,3,3}};
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = GetBotPlayerWithResultForScore(Scores.Kniffel, 0);
            SetValueForScore(botPlayer, Scores.LargeStraight);
            SetValueForScore(botPlayer, Scores.Kniffel);
            SetValueForScore(botPlayer, Scores.ThreeOfAKind);
            SetValueForScore(botPlayer, Scores.Threes);

            botPlayer.AiFixDice(game);
            
            game.Received().FixAllDice(3,true);
        }
        
        [Fact]
        public void IfBotGotTwoSameDiceAndNeedsCorrespondingNumericThenHeFixesThem()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){3,4,1,3,3}};
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = GetBotPlayerWithResultForScore(Scores.Kniffel, 0);
            SetValueForScore(botPlayer, Scores.LargeStraight);
            SetValueForScore(botPlayer, Scores.Kniffel);
            SetValueForScore(botPlayer, Scores.ThreeOfAKind);
            SetValueForScore(botPlayer, Scores.FourOfAKind);
            
            botPlayer.AiFixDice(game);
            
            game.Received().FixAllDice(3,true);
        }
        
        [Fact]
        public void IfBotGotTwoSameDiceButHasNothingToFillThenHeDoesNotFixThem()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){4,4,1,4,2}};
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = GetBotPlayerWithResultForScore(Scores.Kniffel, 0);
            SetValueForScore(botPlayer, Scores.LargeStraight);
            SetValueForScore(botPlayer, Scores.Kniffel);
            SetValueForScore(botPlayer, Scores.ThreeOfAKind);
            SetValueForScore(botPlayer, Scores.FourOfAKind);
            SetValueForScore(botPlayer, Scores.Chance);
            SetValueForScore(botPlayer, Scores.Fours);
            
            botPlayer.AiFixDice(game);
            
            game.DidNotReceive().FixAllDice(4,true);
        }
        
        [Fact]
        public void IfBotGotDoesNotHaveChanceButHasEverythingElseHeFixesTheValuesHigherThanFour()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){4,5,1,2,2}};
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = GetBotPlayerWithResultForScore(Scores.Kniffel, 0);
            SetValueForScore(botPlayer, Scores.LargeStraight);
            SetValueForScore(botPlayer, Scores.Kniffel);
            SetValueForScore(botPlayer, Scores.ThreeOfAKind);
            SetValueForScore(botPlayer, Scores.FourOfAKind);
            SetValueForScore(botPlayer, Scores.Sixs);
            SetValueForScore(botPlayer, Scores.Fives);
            SetValueForScore(botPlayer, Scores.Fours);
            SetValueForScore(botPlayer, Scores.Threes);
            SetValueForScore(botPlayer, Scores.Twos);
            SetValueForScore(botPlayer, Scores.Ones);
            
            botPlayer.AiFixDice(game);
            
            game.Received().FixAllDice(5,true);
            game.Received().FixAllDice(4,true);
        }
        #endregion
        
        #region FillLogic

        [Fact]
        public void WhenBotGetsKniffelItFillsItRightAway()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){5,5,5,5,5}};
            const Scores score = Scores.Kniffel;
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = Substitute.For<IPlayer>();
            var result = GetResultWithMaxValueForScore(score);
            botPlayer.GetResultForScore(score).Returns(result);
            
            botPlayer.AiDecideFill(game);
            
            game.Received().ApplyScore(result);
        }
        
        [Fact]
        public void WhenBotGetsFullHouseItFillsItRightAway()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){5,5,5,2,2}};
            const Scores score = Scores.FullHouse;
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = Substitute.For<IPlayer>();
            var result = GetResultWithMaxValueForScore(score);
            botPlayer.GetResultForScore(score).Returns(result);
            
            botPlayer.AiDecideFill(game);
            
            game.Received().ApplyScore(result);
        }
        
        [Fact]
        public void WhenBotGetsAtLeastFourSixsItFillsThemOnLastRoll()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){6,6,6,6,2}};
            const Scores score = Scores.Sixs;
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = Substitute.For<IPlayer>();
            botPlayer.Roll.Returns(3);
            var result = GetRollResultForScoreWithValue(score, 24);
            botPlayer.GetResultForScore(score).Returns(result);
            
            botPlayer.AiDecideFill(game);
            
            game.Received().ApplyScore(result);
        }
        
        [Fact]
        public void WhenBotGetsAtLeastFourSixsItDoesNotFillThemUntilLastRoll()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){6,6,6,6,2}};
            const Scores score = Scores.Sixs;
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = Substitute.For<IPlayer>();
            botPlayer.Roll.Returns(2);
            var result = GetRollResultForScoreWithValue(score, 24);
            botPlayer.GetResultForScore(score).Returns(result);
            
            botPlayer.AiDecideFill(game);
            
            game.DidNotReceive().ApplyScore(result);
        }
        
        [Fact]
        public void WhenBotGetsLargeStraightItFillsItRightAway()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){2,3,4,5,6}};
            const Scores score = Scores.LargeStraight;
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = Substitute.For<IPlayer>();
            var result = GetResultWithMaxValueForScore(score);
            botPlayer.GetResultForScore(score).Returns(result);
            
            botPlayer.AiDecideFill(game);
            
            game.Received().ApplyScore(result);
        }
        
        [Fact]
        public void WhenBotGetsSmallStraightItFillsItOnLastRoll()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){1,3,4,5,6}};
            const Scores score = Scores.SmallStraight;
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = Substitute.For<IPlayer>();
            botPlayer.Roll.Returns(3);
            var result = GetResultWithMaxValueForScore(score);
            botPlayer.GetResultForScore(score).Returns(result);
            
            botPlayer.AiDecideFill(game);
            
            game.Received().ApplyScore(result);
        }
        
        [Fact]
        public void WhenBotGetsSmallStraightItDoesNotFillItUntilLastRoll()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){1,3,4,5,6}};
            const Scores score = Scores.SmallStraight;
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = Substitute.For<IPlayer>();
            botPlayer.Roll.Returns(2);
            var result = GetResultWithMaxValueForScore(score);
            botPlayer.GetResultForScore(score).Returns(result);
            
            botPlayer.AiDecideFill(game);
            
            game.DidNotReceive().ApplyScore(result);
        }
        
        [Fact]
        public void WhenBotGetsThreeOfKindWithReasonableValueItFillsItOnLastRoll()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){4,4,4,5,6}};
            const Scores score = Scores.ThreeOfAKind;
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = Substitute.For<IPlayer>();
            botPlayer.Roll.Returns(3);
            var result = GetRollResultForScoreWithValue(score, diceResult.Total);
            botPlayer.GetResultForScore(score).Returns(result);

            botPlayer.AiDecideFill(game);
            
            game.Received().ApplyScore(result);
        }
        
        [Fact]
        public void WhenBotGetsThreeOfKindItDoesNotFillItOnLastRollIfValueIsLow()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){1,1,1,2,3}};
            const Scores score = Scores.ThreeOfAKind;
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = Substitute.For<IPlayer>();
            botPlayer.Roll.Returns(3);
            var result = GetRollResultForScoreWithValue(score, diceResult.Total);
            botPlayer.GetResultForScore(score).Returns(result);

            botPlayer.AiDecideFill(game);
            
            game.DidNotReceive().ApplyScore(result);
        }
        
        [Fact]
        public void WhenBotGetsThreeOfKindWithReasonableValueItDoesNotFillItUntilLastRoll()
        {
            var game = Substitute.For<IGame>();
            var diceResult = new DieResult() { DiceResults = new List<int>(){4,4,4,5,6}};
            const Scores score = Scores.ThreeOfAKind;
            game.LastDiceResult.Returns(diceResult);
            var botPlayer = Substitute.For<IPlayer>();
            botPlayer.Roll.Returns(2);
            var result = GetRollResultForScoreWithValue(score, diceResult.Total);
            botPlayer.GetResultForScore(score).Returns(result);

            botPlayer.AiDecideFill(game);
            
            game.DidNotReceive().ApplyScore(result);
        }
        
        #endregion

        #region PrivateMethods
        private IPlayer GetBotPlayerWithResultForScore(Scores score, int value)
        {
            var botPlayer = Substitute.For<IPlayer>();
            var result = GetRollResultForScoreWithValue(score, value);
            botPlayer.GetResultForScore(score).Returns(result);
            return botPlayer;
        }
        
        private IRollResult GetRollResultForScoreWithValue(Scores score, int value)
        {
            var result = Substitute.For<IRollResult>();
            result.ScoreType.Returns(score);
            result.HasValue.Returns(false);
            result.PossibleValue.Returns(value);
            return result;
        }

        private static IPlayer GetBotPlayerWithMaxResultFor(Scores score)
        {
            var botPlayer = Substitute.For<IPlayer>();
            var result = GetResultWithMaxValueForScore(score);
            botPlayer.GetResultForScore(score).Returns(result);
            return botPlayer;
        }

        private static IRollResult GetResultWithMaxValueForScore(Scores score)
        {
            var result = Substitute.For<IRollResult>();
            var maxValue = new RollResult(score, Rules.krSimple).MaxValue;
            result.ScoreType.Returns(score);
            result.HasValue.Returns(false);
            result.PossibleValue.Returns(maxValue);
            result.MaxValue.Returns(maxValue);
            return result;
        }

        private void SetValueForScore(IPlayer player, Scores score)
        {
            var result = Substitute.For<IRollResult>();
            var maxValue = new RollResult(score, Rules.krSimple).MaxValue;
            result.ScoreType.Returns(score);
            result.HasValue.Returns(true);
            result.Value.Returns(maxValue);
            result.MaxValue.Returns(maxValue);
            player.GetResultForScore(score).Returns(result);
            player.IsScoreFilled(score).Returns(true);
        }
        #endregion
    }
}