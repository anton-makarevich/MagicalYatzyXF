using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.Api
{
    public interface IApiClient
    {
        Task<IPlayer> LoginUserAsync(string username, string password);

        Task SaveScoreAsync(string playerName, int score, Rules rule);
    }
}