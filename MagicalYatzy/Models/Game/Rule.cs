using System.Collections.Generic;

namespace Sanet.MagicalYatzy.Models.Game
{
    public class Rule
    {
        public Rule(Rules rule)
        {
            CurrentRule = rule;
        }

        #region Properties
        /// <summary>
        /// Rule
        /// </summary>
        private Rules _Rule;
        public Rules CurrentRule
        {
            get { return _Rule; }
            set
            {
                if (_Rule != value)
                {
                    _Rule = value;
                    //NotifyPropertyChanged("Rule");
                    //NotifyPropertyChanged("RuleNameLocalized");
                }
            }
        }

        /// <summary>
        /// Set of Specific combinations for rule
        /// </summary>
        public List<Scores> ScoresForRule
        {
            get
            {
                switch (CurrentRule)
                {
                    case Rules.krBaby:
                        return new List<Scores>
                        {
                            Scores.Ones,
                            Scores.Twos,
                            Scores.Threes,
                            Scores.Fours,
                            Scores.Fives,
                            Scores.Sixs,
                            Scores.Kniffel
                        };
                    case Rules.krSimple:
                        return new List<Scores>
                        {
                            Scores.Ones,
                            Scores.Twos,
                            Scores.Threes,
                            Scores.Fours,
                            Scores.Fives,
                            Scores.Sixs,
                            Scores.ThreeOfAKind,
                            Scores.FourOfAKind,
                            Scores.FullHouse,
                            Scores.SmallStraight,
                            Scores.LargeStraight,
                            Scores.Total,
                            Scores.Kniffel
                        };
                    default:
                        return new List<Scores>
                        {
                            Scores.Ones,
                            Scores.Twos,
                            Scores.Threes,
                            Scores.Fours,
                            Scores.Fives,
                            Scores.Sixs,
                            Scores.Bonus,
                            Scores.ThreeOfAKind,
                            Scores.FourOfAKind,
                            Scores.FullHouse,
                            Scores.SmallStraight,
                            Scores.LargeStraight,
                            Scores.Total,
                            Scores.Kniffel
                        };
                }

            }
        }
        /// <summary>
        /// Maximum moves count based on rules
        /// </summary>
        public int MaxMove
        {
            get
            {
                switch (CurrentRule)
                {
                    case Rules.krBaby:
                        return 7;
                }
                return 13;
            }
        }


        /// <summary>
        /// Helper method to get if we play with extended bonuses
        /// </summary>
        public bool HasExtendedBonuses
        {
            get
            {
                return CurrentRule == Rules.krExtended || CurrentRule == Rules.krMagic;
            }
        }

        /// <summary>
        /// Helper method to get if we play with standard bonuses
        /// </summary>
        public bool HasStandardBonus
        {
            get
            {
                return CurrentRule == Rules.krStandard || CurrentRule == Rules.krExtended || CurrentRule == Rules.krMagic;
            }
        }


        /// <summary>
        /// returns list of available hands
        /// </summary>
        static public Scores[] PokerHands = new Scores[]
        {
            Scores.ThreeOfAKind,
            Scores.FourOfAKind,
            Scores.FullHouse,
            Scores.SmallStraight,
            Scores.LargeStraight,
            Scores.Kniffel
        };

        #endregion

        #region Methods

        public override string ToString()
        {
            switch (CurrentRule)
            {
                case Rules.krBaby:
                    return "ScoresB";
                case Rules.krExtended:
                    return "ScoresE";
                case Rules.krStandard:
                    return "ScoresS";
                case Rules.krMagic:
                    return "ScoresM";
            }
            return "Scores";
        }

        #endregion
    }
}
