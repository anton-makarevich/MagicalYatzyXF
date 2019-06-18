using System.Threading.Tasks;
using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Services;

namespace Sanet.MagicalYatzy.Web.Functions.Services
{
    public class AzureLeaderBoardService:ILeaderBoardService
    {
        public Task SaveScoreAsync(PlayerScore score)
        {
            throw new System.NotImplementedException();
        }
    }
}