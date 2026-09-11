using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.Game
{
    public interface IGameService
    {
        Task<IGame> CreateNewLocalGameAsync(Rules rule);
        IGame CurrentLocalGame { get; }
    }
}