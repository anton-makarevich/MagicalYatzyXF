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

        public Rules CurrentRule { get; }

        /// <summary>
        /// Set of Specific combinations for rule
        /// </summary>
        public IEnumerable<Scores> ScoresForRule
        {
            get
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
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
        public int MaxRound => (CurrentRule == Rules.krBaby) ? 7 : 13;

        /// <summary>
        /// Helper method to get if we play with extended bonuses
        /// </summary>
        public bool HasExtendedBonuses => CurrentRule == Rules.krExtended || CurrentRule == Rules.krMagic;

        /// <summary>
        /// Helper method to get if we play with standard bonuses
        /// </summary>
        public bool HasStandardBonus => CurrentRule == Rules.krStandard || CurrentRule == Rules.krExtended || CurrentRule == Rules.krMagic;


        /// <summary>
        /// returns list of available hands
        /// </summary>
        public static IEnumerable<Scores> PokerHands => new[]
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
            // ReSharper disable once SwitchStatementMissingSomeCases
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
