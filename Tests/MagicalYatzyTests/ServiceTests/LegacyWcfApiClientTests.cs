using System.Threading.Tasks;
using MagicalYatzyTests.ServiceTests.Game;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Services;
using Xunit;

namespace MagicalYatzyTests.ServiceTests
{
    public class LegacyWcfApiClientTests
    {
        [Fact]
        public async Task LoginCallReturnsUserForValidCreds()
        {
            var sut = new LegacyWcfClient();


            var player = await sut.LoginUserAsync(PlayerServiceTests.TestUserName, PlayerServiceTests.TestUserPassword);

            Assert.NotNull(player);
            Assert.Equal(PlayerServiceTests.TestUserName, player?.Name);
            Assert.NotEqual(PlayerServiceTests.TestUserPassword, player?.Password);
            Assert.Equal(PlayerServiceTests.TestUserPassword, player?.Password?.Decrypt(33));
        }

        [Fact]
        public async Task FailingLoginCallReturnsNullForWrongPassword()
        {
            var sut = new LegacyWcfClient();

            var player = await sut.LoginUserAsync(PlayerServiceTests.TestUserName, "wrongpassword");

            Assert.Null(player);
        }
    }
}
