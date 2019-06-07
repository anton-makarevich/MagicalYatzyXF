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
            var sortedResults = DiceResults.OrderBy(f => f).Distinct().ToList();

            var firstValue = sortedResults.First();
            var amountOfValuesInRow = 1;
            
            var stints = new List<(int,int)>();
            
            for (var resultIndex = 0; resultIndex < sortedResults.Count-1; resultIndex++)
            {
                if (sortedResults[resultIndex + 1] - sortedResults[resultIndex] == 1)
                {
                    amountOfValuesInRow++;
                }
                else
                {
                    stints.Add((firstValue,amountOfValuesInRow));
                    firstValue = sortedResults[resultIndex + 1];
                    amountOfValuesInRow = 1;
                }
            }
            stints.Add((firstValue,amountOfValuesInRow));
            
            return stints.OrderBy(f=>f.Item2).Last();
        }
        
        public List<(int diceValue, int amountOfDice)> CalculateDiceOccurrences()
        {
            return DiceResults.GroupBy(i => i)
                .Select(grp => (grp.Key, grp.Count()))
                .ToList();
        }
    }
}
