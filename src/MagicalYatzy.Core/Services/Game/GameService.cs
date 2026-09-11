using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;

namespace Sanet.MagicalYatzy.Services.Game
{
    public class GameService : IGameService
    {
        private readonly IDiceGenerator _diceGenerator;

        public GameService(IDiceGenerator diceGenerator)
        {
            _diceGenerator = diceGenerator;
        }
        public async Task<IGame> CreateNewLocalGameAsync(Rules rule)
        {
            CurrentLocalGame = await Task.FromResult(new YatzyGame(rule, _diceGenerator) as IGame);
            return CurrentLocalGame;
        }

        public IGame CurrentLocalGame { get; private set; }
    }
}