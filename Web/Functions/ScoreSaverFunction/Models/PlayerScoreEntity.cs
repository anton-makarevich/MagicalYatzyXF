using System;
using Microsoft.WindowsAzure.Storage.Table;
using Sanet.MagicalYatzy.Dto.Models;

namespace Sanet.MagicalYatzy.Web.Functions.ScoreSaver.Models
{
    public class PlayerScoreEntity:TableEntity
    {
        public PlayerScoreEntity()
        {
            
        }

        public PlayerScoreEntity(PlayerScore playerScore)
        {
            ScoreId= Guid.NewGuid().ToString("N");
            
            PartitionKey = playerScore.Rule;
            RowKey = ScoreId;

            Rule = playerScore.Rule;
            Score = playerScore.Score;
            PlayerId = playerScore.PlayerId;
            
            SeasonId = playerScore.SeasonId;
        }

        public string SeasonId { get; set; }

        public string PlayerId { get; set; }

        public int Score { get; set; }

        public string ScoreId { get; set; }

        public string Rule { get; set; }
    }
}