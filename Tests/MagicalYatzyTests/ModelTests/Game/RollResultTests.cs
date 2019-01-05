using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Utils;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class RollResultTests
    {
        private readonly RollResult _sut = new RollResult(Scores.Ones);
        
        [Fact]
        public void HasCorrectMaxValueForSpecifiedScoreType()
        {
            var maxValues = new Dictionary<Scores, int>
            {
                {Scores.Ones, 5},
                {Scores.Twos, 10},
                {Scores.Threes, 15},
                {Scores.Fours, 20},
                {Scores.Fives, 25},
                {Scores.Sixs, 30},
                {Scores.ThreeOfAKind, 30},
                {Scores.FourOfAKind, 30},
                {Scores.FullHouse, 25},
                {Scores.SmallStraight, 30},
                {Scores.LargeStraight, 40},
                {Scores.Total, 30},
                {Scores.Kniffel, 50}
            };
            var allScores = EnumUtils.GetValues<Scores>();

            foreach (var score in allScores)
            {
                var sut = new RollResult(score);
                Assert.Equal(!maxValues.ContainsKey(score) ? 0 : maxValues[score], sut.MaxValue);
            }
        }

        [Fact]
        public void CanSetCorrectValue()
        {
            const int value = 2;
            _sut.Value = value;
            Assert.Equal(value, _sut.Value);
            Assert.True(_sut.HasValue);
        }

        [Fact]
        public void CannotSetValueLessThanZero()
        {
            const int value = -1;
            _sut.Value = value;
            Assert.Equal(0, _sut.Value);
            Assert.False(_sut.HasValue);
        }
        
        [Fact]
        public void CannotSetValueMoreThanMaxValue()
        {
            const int value = 6;
            _sut.Value = value;
            Assert.Equal(0, _sut.Value);
            Assert.False(_sut.HasValue);
        }

        [Fact]
        public void IsZeroValueIsFalseUntilValueIsNotSet()
        {
            Assert.False(_sut.IsZeroValue);
        }
        
        [Fact]
        public void IsZeroValueIsTrueWhenValueIsSetToZero()
        {
            _sut.Value = 0;
            Assert.True(_sut.IsZeroValue);
        }

        [Fact]
        public void OnlyOnesTwosThreesFoursFivesSixAreNumericValues()
        {
            var numericScores = new List<Scores>
                {Scores.Ones, Scores.Twos, Scores.Threes, Scores.Fours, Scores.Fives, Scores.Sixs};
            
            var allScores = EnumUtils.GetValues<Scores>();

            foreach (var score in allScores)
            {
                var sut = new RollResult(score);
                Assert.Equal(numericScores.Contains(score) , sut.IsNumeric);
            }
        }
    }
}