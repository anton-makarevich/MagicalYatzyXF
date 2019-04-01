using Xunit;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;

namespace MagicalYatzyTests.ModelTests.Game.DiceGenerator
{
    public class AllOfKindTestGeneratorTests
    {
        [Fact]
        public void GeneratesTheSameValueAsFirstFixedDicePassedAsParameter()
        {
            var fixedDice = new[] { 1 };
            var sut = new AllOfKindTestGenerator();

            for(int i = 0; i<10;i++)
            {
                var value = sut.GetNextDiceResult(fixedDice);
                Assert.Equal(1, value);
            }
        }
    }
}
