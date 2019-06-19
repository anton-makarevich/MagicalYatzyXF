using System.Collections.Generic;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.StorageService
{
    public interface IStorageService
    {
        Task SavePlayersAsync(List<Player> players);
        Task<List<Player>> LoadPlayersAsync(string dataFile = null);
    }
}
