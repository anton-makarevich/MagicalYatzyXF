using System.Threading.Tasks;
using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Services;

namespace Sanet.MagicalYatzy.Web.Functions.ScoreSaver.Services
{
    public class AzureLeaderBoardService:ILeaderBoardService
    {
        public Task<string> SaveScoreAsync(PlayerScore score)
        {
            throw new System.NotImplementedException();
        }
    }
}