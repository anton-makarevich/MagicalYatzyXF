using System.Threading.Tasks;
using Sanet.MagicalYatzy.Services.Game;
using Xunit;

namespace MagicalYatzyTests.ServiceTests.Game
{
    public class GameServiceTests
    {
        [Fact]
        public async Task CreateNewLocalGameReturnsGameObject()
        {
            var sut = new GameService();
            var game = await sut.CreateNewLocalGameAsync();
            Assert.NotNull(game);
        }
    }
}