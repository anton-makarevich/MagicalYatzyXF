using System.Collections.Generic;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.StorageService
{
    public interface IStorageService
    {
        Task SavePlayersAsync(List<IPlayer> players);
        Task<List<IPlayer>> LoadPlayersAsync(string dataFile = null);
    }
}
