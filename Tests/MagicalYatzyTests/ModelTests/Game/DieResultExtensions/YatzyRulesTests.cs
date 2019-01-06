using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DieResultExtensions;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game.DieResultExtensions
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
                var resultForOnes = sut.YatzyOfAKindScore(sameValues);
                Assert.Equal(11, resultForOnes);
            }
        }
        
        [Fact]
        public void OfAKindReturnsZeroIfThereAreLessSameValuesThanSpecified()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 2, 2, 2, 4}};
            
            for (var sameValues = 4; sameValues < 6; sameValues++)
            {
                var resultForOnes = sut.YatzyOfAKindScore(sameValues);
                Assert.Equal(0, resultForOnes);
            }
        }
    }
}