using System;

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

        public ScoreStatus Status =>
            (HasBonus) 
                ? ScoreStatus.Bonus 
                : (HasValue) 
                    ? ScoreStatus.Value 
                    : ScoreStatus.NoValue;
        
        public bool IsNumeric =>
            (ScoreType == Scores.Ones ||
             ScoreType == Scores.Twos ||
             ScoreType == Scores.Threes ||
             ScoreType == Scores.Fours ||
             ScoreType == Scores.Fives ||
             ScoreType == Scores.Sixs);

        public bool IsZeroValue => HasValue && Value == 0;

        public int MaxValue
        {
            get 
            {
                switch (ScoreType)
                {
                    case Scores.Ones:
                        return 5;
                    case Scores.Twos:
                        return 10;
                    case Scores.Threes:
                        return 15;
                    case Scores.Fours:
                        return 20;
                    case Scores.Fives:
                        return 25;
                    case Scores.Sixs:
                        return 30;
                    case Scores.ThreeOfAKind:
                        return 30;
                    case Scores.FourOfAKind:
                        return 30;
                    case Scores.FullHouse:
                        return 25;
                    case Scores.SmallStraight:
                        return 30;
                    case Scores.LargeStraight:
                        return 40;
                    case Scores.Total:
                        return 30;
                    case Scores.Kniffel:
                        return 50;
                }
                return 0;
            }
        }

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