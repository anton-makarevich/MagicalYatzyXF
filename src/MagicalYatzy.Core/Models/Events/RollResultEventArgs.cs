using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Models.Events
{
    public class RollResultEventArgs : PlayerEventArgs
    {
        public int Value { get; }
        public Scores ScoreType { get; }
        public bool HasBonus { get; }

        public RollResultEventArgs(
            IPlayer player, 
            int value,
            Scores scoreType,
            bool hasBonus)
            : base(player)
        {
            Value = value;
            ScoreType = scoreType;
            HasBonus = hasBonus;
        }
    }
}