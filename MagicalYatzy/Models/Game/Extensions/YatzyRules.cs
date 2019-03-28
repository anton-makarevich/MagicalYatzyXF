using System.Linq;

namespace Sanet.MagicalYatzy.Models.Game.Extensions
{
    public static class YatzyRules
    {
        public static int YatzyNumberScore(this DieResult result, int number) =>
            result.DiceResults.Where(f => (f == number)).Sum();

        public static int YatzyOfAKindScore(this DieResult result, int count)
        {
            foreach (var diceResult in result.DiceResults)
            {
                if (result.DiceResults.Count(f => f == diceResult) >= count)
                {
                    return result.Total;
                }
            }
            return 0;
        }

        public static int YatzyFiveOfAKindScore(this DieResult result)
        {
            const int score = 50;
            
            return result.YatzyOfAKindScore(5) > 0 ? score : 0;
        }

        public static int YatzyChanceScore(this DieResult result)
        {
            return result.Total;
        }

        public static int YatzySmallStraightScore(this DieResult result)
        {
            const int score = 30;

            return result.DiceResults.Distinct().Count() >= 4 
                   && result.DiceResults.Contains(3) 
                   && result.DiceResults.Contains(4)
                ? score 
                : 0;
        }

        public static int YatzyLargeStraightScore(this DieResult result)
        {
            const int score = 40;
            
            return result.DiceResults.Distinct().Count() == 5 ? score : 0;
        }

        public static int YatzyFullHouseScore(this DieResult result)
        {
            const int score = 25;
            
            return result.DiceResults.Distinct().Count() == 2 && result.YatzyOfAKindScore(4) == 0 ? score : 0;
        }
    }
}