using System;

namespace Sanet.MagicalYatzy.Dto.Models
{
    public class PlayerScore
    {
        public string ScoreId { get; set; } = Guid.NewGuid().ToString("N");
        public string PlayerName { get; set; } = "";
        public int Score { get; set; }
        public string Rule { get; set; } = "";
        public string? SeasonId { get; set; }
    }
}