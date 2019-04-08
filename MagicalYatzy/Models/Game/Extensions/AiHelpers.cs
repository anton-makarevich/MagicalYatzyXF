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
            int[] n = new int[7];
            
            // the amount of dice with every value 
            for (int i = 1; i <= 6; i++)
            {
                n[i] = game.LastDiceResult.YatzyNumberScore(i) / i;
            }
            
            IRollResult result;
            // check for 3 fives or sixs
            for (int i = 6; i >= 5; i += -1)
            {
                result = player.GetResultForScore((Scores)i);
                if (result != null && !result.HasValue && n[i] > 2)
                {
                    game.FixAllDice(i, true);
                    return;
                }
            }

            // check if we need in row values (no large straight) 
            result = player.GetResultForScore(Scores.LargeStraight);
            if (result != null && !result.HasValue)
            {
                var inRowValues = game.LastDiceResult.XInRow();
                int count= inRowValues.numberOfValuesInRow; //count of dices in row (3 || 4)
                int first = inRowValues.firstValue; //first dice in row
                if (first > 0)
                {
                    for (int i = first; i < first + count; i++)
                    {
                        if (!game.IsDiceFixed(i))
                            game.FixDice(i, true);
                    }
                    return;
                }
            }
            
            // check for full house
            result = player.GetResultForScore(Scores.FullHouse);
            if (result != null && !result.HasValue)
            {
                if (game.LastDiceResult.NumPairs()==2)
                {
                    for (int j = 1; j <= 6; j++)
                    {
                        if (n[j] > 1)
                            game.FixAllDice(j, true);
                    }
                    return;
                }
            }

            for (int j = 5; j >= 1; j += -1)
            {
                for (int i = 6; i >= 1; i += -1)
                {
                    //not clear what does this condition do
                    //if more then 2 same or numerics then fix (why?) 
                    if (j > 2 | player.AllNumericFilled)
                    {
                        if (!player.IsScoreFilled(Scores.Kniffel) |
                            !player.IsScoreFilled(Scores.ThreeOfAKind) |
                            !player.IsScoreFilled(Scores.FourOfAKind) |
                            !player.IsScoreFilled((Scores)i))
                        {
                            if (n[i] == j)
                            {
                                game.FixAllDice(i,true);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (!player.IsScoreFilled((Scores)i))
                        {
                            if (n[i] == j)
                            {
                                game.FixAllDice(i, true);
                                return;
                            }
                        }
                    }
                }
            }
            if (!player.IsScoreFilled(Scores.Chance))
            {
                for (int i = 6; i >= 4; i += -1)
                {
                    game.FixAllDice(i, true);
                }
            }
        }
    }
}