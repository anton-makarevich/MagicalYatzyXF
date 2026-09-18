using Sanet.MagicalYatzy.Models.Game.Extensions;

namespace Sanet.MagicalYatzy.Models.Game
{
    public class RollResult : IRollResult
    {
        private readonly Rules _rule;

        public RollResult(Scores score, Rules rule)
        {
            _rule = rule;
            ScoreType = score;
        }

        public bool HasBonus
        {
            get
            {
                if (!HasValue)
                    return false;
                if (!new Rule(_rule).HasExtendedBonuses)
                    return false;
                return ScoreType != Scores.Kniffel && field;
            }
            set;
        }

        public bool HasValue { get; private set; }
        public bool IsMaxPossibleValue => !HasValue && PossibleValue == MaxValue && PossibleValue != 0;

        public ScoreStatus Status =>
            (HasBonus) 
                ? ScoreStatus.Bonus 
                : (HasValue) 
                    ? ScoreStatus.Value 
                    : ScoreStatus.NoValue;
        
        public bool IsNumeric => ScoreType.IsNumeric();

        public bool IsZeroValue => HasValue && Value == 0;

        public int MaxValue => ScoreType.GetMaxValue();

        public int PossibleValue
        {
            get;
            set
            {
                if (value < 0 || value > MaxValue) return;
                field = value;
            }
        }

        public Scores ScoreType { get; }

        public int Value
        {
            get;
            set
            {
                if (value < 0 || value > MaxValue) return;
                field = value;
                HasValue = true;
            }
        }
    }
}