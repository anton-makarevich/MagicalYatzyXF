using System;
using System.Threading.Tasks;
using MagicalYatzyTests.Services.Game;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services.Api;
using Xunit;

namespace MagicalYatzyTests.Services.Api
{
    public class LegacyWcfApiClientTests
    {
        private readonly LegacyWcfClient _sut;

        public LegacyWcfApiClientTests()
        {
            _sut = new LegacyWcfClient();
        }

        [Fact]
        public async Task LoginCallReturnsUserForValidCreds()
        {
            var player = await _sut.LoginUserAsync(PlayerServiceTests.TestUserName, PlayerServiceTests.TestUserPassword);

            Assert.NotNull(player);
            Assert.Equal(PlayerServiceTests.TestUserName, player.Name);
            Assert.NotEqual(PlayerServiceTests.TestUserPassword, player.Password);
            Assert.Equal(PlayerServiceTests.TestUserPassword, player.Password?.Decrypt(33));
        }

        [Fact]
        public async Task FailingLoginCallReturnsNullForWrongPassword()
        {
            var player = await _sut.LoginUserAsync(PlayerServiceTests.TestUserName, "wrongpassword");

            Assert.Null(player);
        }

        [Fact]
        public async Task SaveScoreIsNotImplemented()
        {
            await Assert.ThrowsAsync<NotImplementedException>(
                () => _sut.SaveScoreAsync("", 0, Rules.krBaby));
        }
    }
}
