using Sanet.MagicalYatzy.Models.Game;
using Xunit;
using Sanet.MagicalYatzy.Utils;
using Sanet.MagicalYatzy.Extensions;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class PlayerTests
    {
        private readonly Player _sut;

        public PlayerTests()
        {
            _sut = new Player();
        }

        [Fact]
        public void DefaultPlayerIsHuman()
        {
            Assert.True(_sut.IsHuman);
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

        [Fact]
        public void PlayersHashcodeIsBasedOnNameAndEncodedPassword()
        {
            int expectedHashcode = $"player{_sut.Name}{_sut.Password.Decrypt(33)}".GetHashCode();
            Assert.Equal(expectedHashcode, _sut.GetHashCode());
        }
    }
}
