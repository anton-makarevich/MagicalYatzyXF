using System.Threading.Tasks;
using Sanet.MagicalYatzy.Dto.Models;

namespace Sanet.MagicalYatzy.Dto.Services
{
    public interface ILeaderBoardService
    {
        Task SaveScoreAsync(PlayerScore score);
    }
}