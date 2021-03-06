using System.Linq;

namespace Sanet.MagicalYatzy.Models.Game.Ai.Extensions
{
    public static class AiHelpers
    {
        public static int NumPairs(this DieResult result)
        {
            var occurrences = new int[7];
            var numberOfPairs = 0;

            foreach (var diceValue in result.DiceResults)
            {
                occurrences[diceValue] += 1;
            }

            for (var i = 0; i <= 6; i++)
            {
                if (occurrences[i] > 1)
                {
                    numberOfPairs++;
                }
            }

            return numberOfPairs;
        }
        
        public static int MinAllowableValue(this IRollResult result)
        {
            switch (result.ScoreType)
            {
                case Scores.Ones:
                    return 1;
                case Scores.Twos:
                    return 2;
                case Scores.Threes:
                    return 3;
                case Scores.Fours:
                    return 4;
                case Scores.Fives:
                    return 5;
                case Scores.Sixs:
                    return 6;
                case Scores.ThreeOfAKind:
                    return 22;
                case Scores.FourOfAKind:
                    return 17;
                case Scores.FullHouse:
                    return 25;
                case Scores.SmallStraight:
                    return 30;
                case Scores.LargeStraight:
                    return 40;
                case Scores.Chance:
                    return 25;
                case Scores.Kniffel:
                    return 50;
            }

            return 0;
        }
        
        public static (int oldValue, int newValue) AiDecideDiceChange(
            this DieResult diceResult, 
            bool needsSmallStraight, 
            bool needsLargeStraight)
        {
            int oldValue, newValue;
            var diceOccurrences = diceResult.CalculateDiceOccurrences();
            var sortedResults = diceResult.DiceResults.OrderBy(d => d).ToList();
            var (firstValue, countInRow) = diceResult.CalculateInRowDice();
            if ((needsSmallStraight && countInRow == 3) || (needsLargeStraight && countInRow == 4))
            {
                oldValue = diceOccurrences.FirstOrDefault(f => f.amountOfDice > 1).diceValue;
                if (oldValue == 0)
                    oldValue = sortedResults.First(i => i < firstValue || i > firstValue + countInRow);
                newValue = (firstValue < 3)
                    ? firstValue + countInRow
                    : firstValue - 1;
                return (oldValue, newValue);
            }
            
            newValue = diceOccurrences
                .Where(f => f.amountOfDice > 1)
                .OrderByDescending(f => f.diceValue)
                .FirstOrDefault().diceValue;
            if (newValue == 0)
                newValue = sortedResults.Last();

            oldValue = sortedResults.First(f=>f!=newValue);
            
            return (oldValue, newValue);
        }
    }
}