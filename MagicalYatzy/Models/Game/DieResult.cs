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
    }
}
