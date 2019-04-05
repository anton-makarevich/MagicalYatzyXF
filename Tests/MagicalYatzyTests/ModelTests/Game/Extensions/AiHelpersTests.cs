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

        private static IPlayer GetBotPlayerWithMaxResultFor(Scores score)
        {
            var botPlayer = Substitute.For<IPlayer>();
            var result = Substitute.For<IRollResult>();
            var maxValue = new RollResult(score, Rules.krSimple).MaxValue;
            result.ScoreType.Returns(score);
            result.HasValue.Returns(false);
            result.PossibleValue.Returns(maxValue);
            result.MaxValue.Returns(maxValue);
            return botPlayer;
        }
    }
}