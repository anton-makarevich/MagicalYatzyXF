using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Services.Game;
using Xunit;

namespace MagicalYatzyTests.Services.Game
{
    public class GameServiceTests
    {
        private readonly IGameService _sut = new GameService(new RandomDiceGenerator());
        [Fact]
        public async Task CreateNewLocalGameReturnsGameObject()
        {
            var game = await _sut.CreateNewLocalGameAsync(Rules.krSimple);
            Assert.NotNull(game);
        }

        [Fact]
        public async Task CreateNewLocalGameAssignsCurrentLocalGame()
        {
            var game = await _sut.CreateNewLocalGameAsync(Rules.krSimple);
            Assert.Equal(game, _sut.CurrentLocalGame);
        }

        [Fact]
        public async Task CurrentLocalGameHasSpecifiedRule()
        {
            const Rules rule = Rules.krBaby;
            await _sut.CreateNewLocalGameAsync(rule);
            
            Assert.Equal(rule,_sut.CurrentLocalGame.Rules.CurrentRule);
        }
    }
}