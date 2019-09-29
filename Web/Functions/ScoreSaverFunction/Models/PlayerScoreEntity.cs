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
            var scoreId= Guid.NewGuid().ToString("N");
            
            PartitionKey = $"{playerScore.SeasonId}{playerScore.Rule}";
            RowKey = scoreId;

            Score = playerScore.Score;
            PlayerName = playerScore.PlayerName;
        }

        public string PlayerName { get; set; }

        public int Score { get; set; }
    }
}