using Sanet.MagicalYatzy.Models.Game;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanet.MagicalYatzy.Services
{
    public interface IStorageService
    {
        Task SavePlayersAsync(List<Player> players);
        Task<List<Player>> LoadPlayersAsync(string dataFile = null);
    }
}
