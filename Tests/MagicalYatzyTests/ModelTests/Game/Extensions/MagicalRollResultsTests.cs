using System;
using System.Linq;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace MagicalYatzyTests.ModelTests.Game.Extensions
{
    public class MagicalRollResults
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public MagicalRollResults(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ReturnsValidResultsForThreeOfAKind()
        {
            const Scores sut = Scores.ThreeOfAKind;

            var diceResults = new DieResult() {DiceResults = sut.GetMagicResults().ToList()};
            Assert.True(diceResults.YatzyOfAKindScore(3)>0);
        }
        
        [Fact]
        public void ReturnsValidResultsForFourOfAKind()
        {
            const Scores sut = Scores.FourOfAKind;
            
            var diceResults = new DieResult() {DiceResults = sut.GetMagicResults().ToList()};
            Assert.True(diceResults.YatzyOfAKindScore(4)>0);
        }
        
        [Fact]
        public void ReturnsValidResultsForFullHouse()
        {
            const Scores sut = Scores.FullHouse;

            var diceResults = new DieResult() {DiceResults = sut.GetMagicResults().ToList()};
            Assert.True(diceResults.YatzyFullHouseScore() > 0);
        }
        
        [Fact]
        public void ReturnsValidResultsForSmallStraight()
        {
            const Scores sut = Scores.SmallStraight;
            
            var diceResults = new DieResult() {DiceResults = sut.GetMagicResults().ToList()};
            Assert.True(diceResults.YatzySmallStraightScore()>0);
        }
        
        [Fact]
        public void ReturnsValidResultsForLargeKniffel()
        {
            const Scores sut = Scores.Kniffel;
            
            var diceResults = new DieResult() {DiceResults = sut.GetMagicResults().ToList()};
            Assert.True(diceResults.YatzyFiveOfAKindScore()>0);
        }
    }
}