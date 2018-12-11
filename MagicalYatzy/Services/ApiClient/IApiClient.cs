using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services
{
    public interface IApiClient
    {
        Task<IPlayer> LoginUserAsync(string username, string password);
    }
}