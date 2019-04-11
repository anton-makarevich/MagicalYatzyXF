using System.Collections.Generic;
using System.Linq;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Utils;

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
                amountOfDiceForValue.Add((diceValue, game.LastDiceResult.YatzyNumberScore(diceValue) / diceValue));
            }

            IRollResult result;
            // check for 3 fives or sixs
            var diceToCheck = amountOfDiceForValue.Where(f => f.diceValue > 4 && f.amountOfDice > 2).ToList();
            foreach (var (diceValue, _) in diceToCheck)
            {
                result = player.GetResultForScore((Scores) diceValue);
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
            foreach (var (diceValue, diceAmount) in amountOfDiceForValue.OrderByDescending(f => f.amountOfDice))
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
            var amountOfDiceForValue = new List<(int diceValue, int amountOfDice)>();

            // the amount of dice with every value 
            for (var diceValue = 1; diceValue <= 6; diceValue++)
            {
                amountOfDiceForValue.Add((diceValue, game.LastDiceResult.YatzyNumberScore(diceValue) / diceValue));
            }

            // check for kniffel
            var result = player.GetResultForScore(Scores.Kniffel);
            if (result != null
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
            if (amountOfDiceForValue.First(f => f.diceValue == 6).amountOfDice > 3)
            {
                result = player.GetResultForScore(Scores.Sixs);
                if (result != null && !result.HasValue && player.Roll == 3)
                {
                    game.ApplyScore(result);
                    return;
                }
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
            for (var scoreIndex = 10; scoreIndex >= 9; scoreIndex += -1)
            {
                result = player.GetResultForScore((Scores) scoreIndex);
                if (result == null || result.HasValue || result.PossibleValue < result.MinAllowableValue() ||
                    result.PossibleValue <= 0 || scoreIndex - player.Roll >= 8) continue;
                game.ApplyScore(result);
                return;
            }

            // 4 and 3 in a row
            for (var scoreIndex = 8; scoreIndex >= 7; scoreIndex += -1)
            {
                result = player.GetResultForScore((Scores) scoreIndex);
                if (result == null || result.HasValue || player.Roll != 3 ||
                    result.PossibleValue < result.MinAllowableValue() - (game.Round - 1) / 2 ||
                    result.PossibleValue <= 0) continue;
                game.ApplyScore(result);
                return;
            }

            // numerics 
            foreach (var (diceValue, _) in amountOfDiceForValue
                .Where(f=>f.diceValue>0)
                .OrderByDescending(f => f.amountOfDice))
            {
                result = player.GetResultForScore((Scores) diceValue);
                if (result == null 
                    || result.HasValue 
                    || result.PossibleValue == 0
                    || player.Roll != 3) continue;
                game.ApplyScore(result);
                return;
            }

            // chance
            result = player.GetResultForScore(Scores.Chance);
            if (result != null
                && !result.HasValue
                && result.PossibleValue > 0
                && player.Roll == 3
                && result.PossibleValue >= result.MinAllowableValue() - (game.Round - 1) / 2)
            {
                game.ApplyScore(result);
                return;
            }

            //once again 4 and 3 in row
            for (var scoreIndex = 8; scoreIndex >= 7; scoreIndex += -1)
            {
                result = player.GetResultForScore((Scores) scoreIndex);
                if (result == null || result.HasValue || result.PossibleValue <= 0 || player.Roll != 3) continue;
                game.ApplyScore(result);
                return;
            }

            //if not filled - filling at least anything including 0
            for (var scoreIndex = 1; scoreIndex <= 13; scoreIndex++)
            {
                result = player.GetResultForScore((Scores) scoreIndex);
                if (result == null || result.HasValue) continue;
                game.ApplyScore(result);
                return;
            }
        }

        public static void AiDecideRoll(this IPlayer player, IGame game)
        {
            if (game.Rules.CurrentRule == Rules.krMagic)
            {
                if (player.CanUseArtifact(Artifacts.MagicalRoll) && game.Round == game.Rules.MaxRound)
                {
                    var hasFreeHand = Rule.PokerHands.Any(score => !player.GetResultForScore(score).HasValue);
                    if (hasFreeHand)
                    {
                        game.ReportMagicRoll();
                        return;
                    }
                }
                if (player.MagicalArtifactsForGame.Any() && player.Roll == 3)
                {
                    var numericHands = EnumUtils.GetValues<Scores>().Where(f => f.IsNumeric()).ToList();
                    numericHands.Add(Scores.ThreeOfAKind);
                    numericHands.Add(Scores.FourOfAKind);
                    var areAllNumericFilled = true;
                    foreach (var score in numericHands)
                    {
                        var result = player.GetResultForScore(score);
                        if (result != null && result.HasValue) continue;
                        areAllNumericFilled = false;
                        break;
                    }

                    if (player.CanUseArtifact(Artifacts.MagicalRoll) && areAllNumericFilled)
                    {
                        game.ReportMagicRoll(); 
                    }
                    return;
                }
            }
            game.ReportRoll();
        }
    }
}