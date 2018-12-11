using NUnit.Framework;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalYatzyTests.ServiceTests
{
    public class LocalJsonStorageServiceTests
    {
        [Test]
        public async Task StorageServiceShouldLoadSavedPlayers()
        {
            // Arange
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
            Assert.AreEqual(playersToSave, loadedPlayers);
        }
    }
}
