using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DieResultExtensions;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game.DieResultExtensions
{
    public class YatzyRulesTests
    {
        [Fact]
        public void ReturnsCorrectScoreForNumericValues()
        {
            var dieResult = new DieResult {DiceResults = new List<int> {1, 2, 2, 4}};
            var resultForOnes = dieResult.KniffelNumberScore(1);
            Assert.Equal(1,resultForOnes);
            
            var resultForTwos = dieResult.KniffelNumberScore(2);
            Assert.Equal(4,resultForTwos);
            
            var resultForThrees = dieResult.KniffelNumberScore(3);
            Assert.Equal(0,resultForThrees);
            
            var resultForFours = dieResult.KniffelNumberScore(4);
            Assert.Equal(4,resultForFours);
        }
    }
}