using System.Threading.Tasks;
using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Services;

namespace Sanet.MagicalYatzy.Web.Functions.ScoreSaver.Services
{
    public class AzureLeaderBoardService:ILeaderBoardService
    {
//        const string TableName = "ScoresTable";
//        private readonly string _connectionString = Environment.GetEnvironmentVariable("TableConnection");
//        CloudStorageAccount _storageAccount; 
//        CloudTableClient tableClient; 
//        CloudTable customerTable;

        public AzureLeaderBoardService()
        {
            //_storageAccount = CloudStorageAccount.Parse(_connectionString); 
        }
        
        public Task<string> SaveScoreAsync(PlayerScore score)
        {
            //throw new System.NotImplementedException();
            return null;
        }
    }
}