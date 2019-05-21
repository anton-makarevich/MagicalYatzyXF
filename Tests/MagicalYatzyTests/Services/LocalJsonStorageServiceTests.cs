using System.Collections.Generic;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Xunit;

namespace MagicalYatzyTests.Services
{
    public class LocalJsonStorageServiceTests
    {
        [Fact]
        public async Task StorageServiceLoadsSavedPlayers()
        {
            // Arrange
            var sut = new LocalJsonStorageService();

            var playersToSave = new List<Player>
            {
                new Player{ Name = "Player 1", Password = "1234"},
                new Player{ Name = "Player 2", Password = "1234"}
            };

            // Act
            await sut.SavePlayersAsync(playersToSave);
            var loadedPlayers = await sut.LoadPlayersAsync();

            //Assert
            Assert.Equal(playersToSave, loadedPlayers);
        }
    }
}
