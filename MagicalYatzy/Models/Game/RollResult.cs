using System;
using Sanet.MagicalYatzy.Models.Game.Extensions;

namespace Sanet.MagicalYatzy.Models.Game
{
    public class RollResult : IRollResult
    {
        private readonly Rules _rule;
        private int _value;
        private bool _hasBonus;
        private int _possibleValue;

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
                return ScoreType != Scores.Kniffel && _hasBonus;
            }
            set => _hasBonus = value;
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
            get => _possibleValue;
            set
            {
                if (value< 0 || value > MaxValue) return;
                _possibleValue = value;
            }
        }

        public Scores ScoreType { get; }

        public int Value
        {
            get => _value;
            set
            {
                if (value< 0 || value > MaxValue) return;
                _value = value;
                HasValue = true;
            }
        }
    }
}