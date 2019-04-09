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

            // fix same if needs kniffel of a kind numerics or chance
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
        
        public static void AiDecideFill(this IPlayer player, IGame game)
        {
            //this has been converted from my old kniffel game

            int[] n = new int[7];

            // how many dice of every value
            for (int i = 1; i <= 6; i++)
            {
                n[i] = game.LastDiceResult.YatzyNumberScore(i) / i;
            }
            
            // check for kniffel
            var result=player.GetResultForScore(Scores.Kniffel);
            if (result!=null 
                && !result.HasValue 
                && result.PossibleValue == result.MaxValue 
                && result.PossibleValue > 0)
            {
                game.ApplyScore(result);
                return;
            }

            // check full house
            result = player.GetResultForScore(Scores.FullHouse);
            if (result != null 
                && !result.HasValue 
                && result.PossibleValue == result.MaxValue
                && result.PossibleValue > 0)
            {
                game.ApplyScore(result);
                return;
            }
            
            // sixs if at least 4 of them and no value 
            result = player.GetResultForScore(Scores.Sixs);
            if (result != null && !result.HasValue && n[6] == 4 && player.Roll == 3)
            {
                game.ApplyScore(result);
                return;
            }

            // check for LS
            result = player.GetResultForScore(Scores.LargeStraight);
            if (result != null 
                && !result.HasValue 
                && result.PossibleValue >= result.MinAllowableValue()
                && result.PossibleValue > 0)
            {
                game.ApplyScore(result);
                return;
            }

            //checking  SS and FH
            for (int i = 10; i >= 9; i += -1)
            {
                result = player.GetResultForScore((Scores)i);
                if (result != null && !result.HasValue && result.PossibleValue >= result.MinAllowableValue() && i-player.Roll<8) //last condition - fill SS only on third roll, and FH - not on first
                {
                    game.ApplyScore(result);
                    return;
                }
            }
            
            // 4 and 3 in a row
            for (int i = 8; i >= 7; i += -1)
            {
                result = player.GetResultForScore((Scores)i);
                if (result != null && !result.HasValue && result.PossibleValue >= result.MinAllowableValue() - (game.Round - 1) / 2)
                {
                    game.ApplyScore(result);
                    return;
                }
            }
            
            // numerics 
            for (int j = 5; j >= 1; j += -1)
            {
                //Step -1
                for (int i = 1; i <= 6; i++)
                {
                    result = player.GetResultForScore((Scores)i);
                    if (result != null && !result.HasValue && n[i] >= j)
                    {
                        game.ApplyScore(result);
                        return;
                    }
                }
            }
            
            //chance
            result = player.GetResultForScore(Scores.Chance);
            if (result != null && !result.HasValue && result.PossibleValue >= result.MinAllowableValue() - (game.Round - 1) / 2)
            {
                game.ApplyScore(result);
                return;
            }
            
            //once again 4 and 3 in row
            for (int i = 8; i >= 7; i += -1)
            {
                result = player.GetResultForScore((Scores)i);
                if (result != null && !result.HasValue && result.PossibleValue >  0)
                {
                    game.ApplyScore(result);
                    return;
                }
            }
            
            //if not filled - filling at least anything including 0
            for (int i = 1; i <= 13; i++)
            {
                result = player.GetResultForScore((Scores)i);
                if (result != null && !result.HasValue)
                {
                    game.ApplyScore(result);
                    return;
                }
            }
        }
    }
}