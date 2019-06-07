using System.Collections.Generic;
using System.Linq;

namespace Sanet.MagicalYatzy.Models.Game
{
    /// <summary>
    /// array of dice values with helpers
    /// </summary>
    public class DieResult
    {
        public List<int> DiceResults { get; set; }
        public int Total
        {
            get
            {
                if (DiceResults != null)
                    return DiceResults.Sum();
                return 0;
            }
        }
        public int NumDice
        {
            get
            {
                if (DiceResults != null)
                    return DiceResults.Count;
                return 0;
            }
        }

        public int NumDiceOf(int value)
        {
            return DiceResults.Count(f => f == value);
        }
        
        public (int firstValue, int numberOfValuesInRow) CalculateInRowDice()
        {
            var occurrences = new int[7];
            var count = 3;
            foreach (var res in DiceResults)
            {
                occurrences[res] += 1;
            }

            for (var i = 1; i < 5; i++)
                if (occurrences[i] >= 1 & occurrences[i + 1] >= 1 & occurrences[i + 2] >= 1)
                {
                    if (i >= 4 || occurrences[i + 3] < 1) return (i, count);
                    count = 4;
                    if (i < 3 && occurrences[i + 4] >= 1)
                        count = 5;

                    return (i, count);
                }

            return (0, 0);
        }
        
        public List<(int diceValue, int amountOfDice)> CalculateDiceOccurrences()
        {
            return DiceResults.GroupBy(i => i)
                .Select(grp => (grp.Key, grp.Count()))
                .ToList();
        }
    }
}
