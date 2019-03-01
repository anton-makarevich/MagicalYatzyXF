using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.Game
{
    public class GameService : IGameService
    {
        public async Task<IGame> CreateNewLocalGameAsync(Rules rule)
        {
            CurrentLocalGame = await Task.FromResult(new YatzyGame(rule) as IGame);
            return CurrentLocalGame;
        }

        public IGame CurrentLocalGame { get; private set; }
    }
}