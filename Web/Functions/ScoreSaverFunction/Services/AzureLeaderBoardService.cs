using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Services;
using Sanet.MagicalYatzy.Web.Functions.ScoreSaver.Models;

namespace Sanet.MagicalYatzy.Web.Functions.ScoreSaver.Services
{
    public class AzureLeaderBoardService:ILeaderBoardService
    {
        private const string TableName = "ScoresTable";

        private readonly CloudTable _scoresTable;

        public AzureLeaderBoardService()
        {
            var connectionString = Environment.GetEnvironmentVariable("TableConnectionString");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _scoresTable = tableClient.GetTableReference(TableName);
        }
        
        public async Task<string> SaveScoreAsync(PlayerScore score)
        {
            var entity = new PlayerScoreEntity(score);
            var insertOperation = TableOperation.InsertOrMerge(entity);
            var result = await _scoresTable.ExecuteAsync(insertOperation);
            var insertedEntity = result.Result as PlayerScoreEntity;

            return insertedEntity?.ScoreId;
        }
    }
}