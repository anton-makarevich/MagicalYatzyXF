using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Xunit;

namespace MagicalYatzyTests.Models.Game.Extensions
{
    public class DiceStyleExtensionsTests
    {
        [Fact]
        public void ReturnsCorrectPathComponentForDiceStyle()
        {
            var sut = DiceStyle.Blue;
            var pathComponent = sut.ToPathComponent();
            Assert.Equal("_2.",pathComponent);
            
            sut = DiceStyle.Red;
            pathComponent = sut.ToPathComponent();
            Assert.Equal("_1.",pathComponent);
            
            sut = DiceStyle.Classic;
            pathComponent = sut.ToPathComponent();
            Assert.Equal("_0.",pathComponent);
        }
    }
}