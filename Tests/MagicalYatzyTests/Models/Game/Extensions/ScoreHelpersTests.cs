using System.Collections.Generic;
using System.Linq;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Sanet.MagicalYatzy.Utils;
using Xunit;

namespace MagicalYatzyTests.Models.Game.Extensions
{
    public class ScoreHelpersTests
    {
        [Fact]
        public void OnesTwosThreesFoursFivesAndSixsAreOnlyNumericHands()
        {
            var numericScores = new[]
            {
                Scores.Ones,
                Scores.Twos,
                Scores.Threes,
                Scores.Fours,
                Scores.Fives,
                Scores.Sixs
            };

            foreach (var score in EnumUtils.GetValues<Scores>())
            {       
                Assert.Equal(numericScores.Contains(score),score.IsNumeric());
            }
        }

        [Fact]
        public void ScoreHasCorrectMaxValueForCorrespondingHand()
        {
            var maxValues = new Dictionary<Scores, int>
            {
                {Scores.Ones, 5},
                {Scores.Twos, 10},
                {Scores.Threes, 15},
                {Scores.Fours, 20},
                {Scores.Fives, 25},
                {Scores.Sixs, 30},
                {Scores.Bonus, 35},
                {Scores.ThreeOfAKind, 30},
                {Scores.FourOfAKind, 30},
                {Scores.FullHouse, 25},
                {Scores.SmallStraight, 30},
                {Scores.LargeStraight, 40},
                {Scores.Chance, 30},
                {Scores.Kniffel, 50}
            };
            
            foreach (var score in EnumUtils.GetValues<Scores>())
            {       
                Assert.Equal(!maxValues.ContainsKey(score) ? 0 : maxValues[score], score.GetMaxValue());
            }
        }
    }
}