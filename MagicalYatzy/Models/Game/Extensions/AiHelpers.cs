using System.Collections.Generic;
using System.Linq;

namespace Sanet.MagicalYatzy.Models.Game.Extensions
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
        
        public static (int firstValue,int numberOfValuesInRow) XInRow(this DieResult result)
        {
            var occurrences = new int[7];
            var count = 3;
            foreach (var res in result.DiceResults)
            {
                occurrences[res] += 1;
            }
            for (var i = 1; i<5; i++)
                if (occurrences[i] >= 1 & occurrences[i+1] >= 1 & occurrences[i+2] >= 1)
                {
                    if (i >= 4 || occurrences[i + 3] < 1) return (i, count);
                    count = 4;
                    if (i < 3 && occurrences[i + 4] >= 1)
                        count = 5;

                    return (i,count);
                }

            return (0,0);
        }

        public static int MinAllowableValue(this RollResult result)
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
        
        public static bool AiNeedsToRollAgain(this IPlayer player)
        {
            var result = player.GetResultForScore(Scores.Kniffel);
            if (result != null && !result.HasValue && result.PossibleValue == result.MaxValue)
                return false;
            
            result = player.GetResultForScore(Scores.FullHouse);
            if (result != null && !result.HasValue && result.PossibleValue == result.MaxValue)
                return false;

            result = player.GetResultForScore(Scores.LargeStraight);
            if (result != null && !result.HasValue)
            {
                if (result.PossibleValue == result.MaxValue)
                    return false;
            }
            
            result = player.GetResultForScore(Scores.SmallStraight);
            return result == null || result.HasValue || result.PossibleValue != result.MaxValue;
        }
        
        public static void AiFixDice(this IPlayer player, IGame game)
        {
            var amountOfDiceForValue = new List<(int diceValue, int amountOfDice)>();
            
            // the amount of dice with every value 
            for (var diceValue = 1; diceValue <= 6; diceValue++)
            {
                amountOfDiceForValue.Add((diceValue,game.LastDiceResult.YatzyNumberScore(diceValue) / diceValue));
            }
            
            IRollResult result;
            // check for 3 fives or sixs
            var diceToCheck = amountOfDiceForValue.Where(f => f.diceValue > 4 && f.amountOfDice > 2).ToList();
            foreach (var (diceValue, _) in diceToCheck)
            {   
                result = player.GetResultForScore((Scores)diceValue);
                if (result == null || result.HasValue) continue;
                game.FixAllDice(diceValue, true);
                return;
            }

            // check if we need in row values (no large straight) 
            result = player.GetResultForScore(Scores.LargeStraight);
            if (result != null && !result.HasValue)
            {
                var (first, count) = game.LastDiceResult.XInRow();
                if (first > 0)
                {
                    for (var diceValue = first; diceValue < first + count; diceValue++)
                    {
                        if (!game.IsDiceFixed(diceValue))
                            game.FixDice(diceValue, true);
                    }
                    return;
                }
            }
            
            // check for full house
            diceToCheck = amountOfDiceForValue.Where(f => f.amountOfDice > 1).ToList();
            var valueTuples = diceToCheck.ToList();
            if (valueTuples.Count() == 2)
            {
                result = player.GetResultForScore(Scores.FullHouse);
                if (result != null && !result.HasValue)
                {
                    foreach (var (diceValue, _) in valueTuples)
                    {
                        game.FixAllDice(diceValue, true);
                    }

                    return;
                }
            }

            foreach (var (diceValue, diceAmount) in amountOfDiceForValue.OrderByDescending(f=>f.amountOfDice))
            {
                if (diceAmount > 2 | player.AllNumericFilled)
                {
                    if (player.IsScoreFilled(Scores.Kniffel) 
                        && player.IsScoreFilled(Scores.ThreeOfAKind) 
                        && player.IsScoreFilled(Scores.FourOfAKind) 
                        && player.IsScoreFilled((Scores) diceValue)) continue;
                    game.FixAllDice(diceValue, true);
                    return;
                }

                if (player.IsScoreFilled((Scores) diceValue)) continue;
                game.FixAllDice(diceValue, true);
                return;
            }

            if (player.IsScoreFilled(Scores.Chance)) return;
            for (var i = 6; i >= 4; i += -1)
            {
                game.FixAllDice(i, true);
            }
        }
    }
}