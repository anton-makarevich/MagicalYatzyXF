using Sanet.MagicalYatzy.Models.Game;
using Xunit;
using Sanet.MagicalYatzy.Utils;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class PlayerTests
    {
        [Fact]
        public void DefaultPlayerIsHuman()
        {
            var sut = new Player();
            Assert.True(sut.IsHuman);
        }

        [Fact]
        public void PlayerTypeIsSetInConstructor()
        {
            var playerTypes = EnumUtils.GetValues<PlayerType>();
            foreach (var type in playerTypes)
            {
                var sut = new Player(type);
                Assert.Equal(type, sut.Type);
            }
        }

        [Fact]
        public void PlayerWithAiTypeIsBot()
        {
            var sut = new Player(PlayerType.AI);
            Assert.True(sut.IsBot);
        }
    }
}
