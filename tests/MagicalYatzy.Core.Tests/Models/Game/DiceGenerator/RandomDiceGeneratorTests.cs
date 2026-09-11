using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Xunit;

namespace MagicalYatzyTests.Models.Game.DiceGenerator
{
    public class RandomDiceGeneratorTests
    {
        private readonly RandomDiceGenerator _sut = new RandomDiceGenerator();

        [Fact]
        public void ResultIsAlwaysPositiveLessThanSeven()
        {
            for (var count = 0; count < 100; count++)
            {
                var result = _sut.GetNextDiceResult();
                Assert.True(result > 0);
                Assert.True(result < 7);
            }
        }

        [Fact]
        public void ShouldReturnDifferentResultInMultipleCalls()
        {
            var results = new List<int>();
            for (var count = 0; count < 100; count++)
            {
                results.Add(_sut.GetNextDiceResult());
            }

            Assert.Contains(1, results);
            Assert.Contains(2, results);
            Assert.Contains(3, results);
            Assert.Contains(4, results);
            Assert.Contains(5, results);
            Assert.Contains(6, results);
        }
    }
}