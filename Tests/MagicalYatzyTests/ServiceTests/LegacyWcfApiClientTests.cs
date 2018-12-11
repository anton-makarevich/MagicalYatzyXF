using NUnit.Framework.Internal;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Services;
using NUnit.Framework;
using Sanet.MagicalYatzy.Extensions;

namespace MagicalYatzyTests.ServiceTests
{
    public class LegacyWcfApiClientTests
    {
        [Test]
        public async Task LoginCallShouldReturnUserForValidCreds()
        {
            var sut = new LegacyWcfClient();


            var player = await sut.LoginUserAsync(PlayerServiceTests.TestUserName, PlayerServiceTests.TestUserPassword);

            Assert.IsNotNull(player);
            Assert.AreEqual(PlayerServiceTests.TestUserName, player?.Name);
            Assert.AreNotEqual(PlayerServiceTests.TestUserPassword, player?.Password);
            Assert.AreEqual(PlayerServiceTests.TestUserPassword, player?.Password?.Decrypt(33));
        }

        [Test]
        public async Task FailingLoginCallShouldReturnNullForWrongPassword()
        {
            var sut = new LegacyWcfClient();

            var player = await sut.LoginUserAsync(PlayerServiceTests.TestUserName, "wrongpassword");

            Assert.IsNull(player);
        }
    }
}
