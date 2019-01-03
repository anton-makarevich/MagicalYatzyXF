using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.Game
{
    public class GameService : IGameService
    {
        public Task<IGame> CreateNewLocalGameAsync()
        {
            return Task.FromResult(new YatzyGame() as IGame);
        }
    }
}