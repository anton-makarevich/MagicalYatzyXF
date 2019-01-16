using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game.Extensions
{
    public class YatzyRulesTests
    {
        [Fact]
        public void NumberScoreReturnsSumOfSpecifiedNumericValues()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 2, 2, 4}};
            
            var resultForOnes = sut.YatzyNumberScore(1);
            Assert.Equal(1,resultForOnes);
            
            var resultForTwos = sut.YatzyNumberScore(2);
            Assert.Equal(4,resultForTwos);
            
            var resultForThrees = sut.YatzyNumberScore(3);
            Assert.Equal(0,resultForThrees);
            
            var resultForFours = sut.YatzyNumberScore(4);
            Assert.Equal(4,resultForFours);
        }

        [Fact]
        public void OfAKindReturnsSumOfAllValuesIfThereAreEqualOrMoreSameValuesThanSpecified()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 2, 2, 2, 4}};

            for (var sameValues = 1; sameValues < 4; sameValues++)
            {
                var result = sut.YatzyOfAKindScore(sameValues);
                Assert.Equal(11, result);
            }
        }
        
        [Fact]
        public void OfAKindReturnsZeroIfThereAreLessSameValuesThanSpecified()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 2, 2, 2, 4}};
            
            for (var sameValues = 4; sameValues < 6; sameValues++)
            {
                var result = sut.YatzyOfAKindScore(sameValues);
                Assert.Equal(0, result);
            }
        }

        [Fact]
        public void FiveOfAKindReturnsFiftyIfThereAreFiveOrMoreSameValues()
        {
            var sut = new DieResult {DiceResults = new List<int>{1,1,1,1,1}};
            var result = sut.YatzyFiveOfAKindScore();
            Assert.Equal(50,result);
        }
        
        [Fact]
        public void FiveOfAKindReturnsZeroIfThereAreLessThanFiveSameValues()
        {
            var sut = new DieResult {DiceResults = new List<int>{1,1,1,1,2}};
            var result = sut.YatzyFiveOfAKindScore();
            Assert.Equal(0,result);
        }
        
        [Fact]
        public void ChanceReturnsSumOfAllValues()
        {
            var sut = new DieResult {DiceResults = new List<int>{1, 2, 2, 2, 4}};
            var result = sut.YatzyChanceScore();
            Assert.Equal(11,result);
        }

        [Fact]
        public void SmallStraightReturnsThirtyIfThereAreFourValuesInRow()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 3, 2, 4, 2}};
            var result = sut.YatzySmallStraightScore();
            Assert.Equal(30,result);
        }
        
        [Fact]
        public void SmallStraightReturnsZeroIfThereAreLessThanFourValuesInRow()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 5, 2, 4, 2}};
            var result = sut.YatzySmallStraightScore();
            Assert.Equal(0,result);
        }
        
        [Fact]
        public void LargeStraightReturnsFortyIfThereAreFiveValuesInRow()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 3, 5, 4, 2}};
            var result = sut.YatzyLargeStraightScore();
            Assert.Equal(40,result);
        }
        
        [Fact]
        public void LargeStraightReturnsZeroIfThereAreLessThanFiveValuesInRow()
        {
            var sut = new DieResult {DiceResults = new List<int> {6, 5, 6, 4, 3}};
            var result = sut.YatzyLargeStraightScore();
            Assert.Equal(0,result);
        }
        
        [Fact]
        public void FullHouseReturnsTwentyFiveWhenThereAreTwoAndThreeSameValues()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 2, 1, 2, 1}};
            var result = sut.YatzyFullHouseScore();
            Assert.Equal(25,result);
        }
        
        [Fact]
        public void FullHouseReturnsZeroWhenThereAreNoTwoAndThreeSameValues()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 1, 1, 2, 1}};
            var result = sut.YatzyFullHouseScore();
            Assert.Equal(0,result);
        }
    }
}