namespace Sanet.MagicalYatzy.Dto.Models
{
    public class PlayerScore
    {
        public string ScoreId { get; set; }
        public string PlayerId { get; set; }
        public int Score { get; set; }
        public string Rule { get; set; }
        public string SeasonId { get; set; }
    }
}