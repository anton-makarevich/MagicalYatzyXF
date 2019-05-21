using System.Collections.Generic;
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

        public static (int firstValue, int numberOfValuesInRow) XInRow(this DieResult result)
        {
            var occurrences = new int[7];
            var count = 3;
            foreach (var res in result.DiceResults)
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

        public static List<(int diceValue, int amountOfDice)> AiCalculatesDiceOccurrences(this DieResult diceResult)
        {
            return diceResult.DiceResults.GroupBy(i => i)
                .Select(grp => (grp.Key, grp.Count()))
                .ToList();
        }
        
        public static (int oldValue, int newValue) AiDecideDiceChange(
            this DieResult diceResult, 
            bool needsSmallStraight, 
            bool needsLargeStraight)
        {
            int oldValue, newValue;
            var diceOccurrences = diceResult.AiCalculatesDiceOccurrences();
            var sortedResults = diceResult.DiceResults.OrderBy(d => d).ToList();
            var (firstValue, countInRow) = diceResult.XInRow();
            if (needsSmallStraight && countInRow == 3 || needsLargeStraight && countInRow == 4)
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