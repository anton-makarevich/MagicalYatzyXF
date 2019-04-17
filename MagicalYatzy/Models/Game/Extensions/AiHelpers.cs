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
            var amountOfDiceForValue = game.LastDiceResult.AiCalculatesDiceOccurrences();

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
            var amountOfDiceForValue = game.LastDiceResult.AiCalculatesDiceOccurrences();

            // check for kniffel
            if (ApplyScoreIfHasMaxValue(Scores.Kniffel, player, game)) return;

            // check full house
            if (ApplyScoreIfHasMaxValue(Scores.FullHouse, player, game)) return;

            IRollResult result;
            // sixs if at least 4 of them and no value 
            if (amountOfDiceForValue.FirstOrDefault(f => f.diceValue == 6).amountOfDice > 3)
            {
                if (ApplyScoreIfLastRound(Scores.Sixs, player, game)) return;
            }

            // check for LS
            if (ApplyScoreIfHasAllowableValue(Scores.LargeStraight, player, game)) return;

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
                if (ApplyScoreIfLastRound((Scores) diceValue, player, game)) return;
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
                if (ApplyScoreIfLastRound((Scores) scoreIndex, player, game)) return;
            }

            //if not filled - filling at least anything including 0
            for (var scoreIndex = 1; scoreIndex <= 13; scoreIndex++)
            {
                if (ApplyScoreWithAnyValue((Scores) scoreIndex, player, game)) return;
            }
        }

        private static bool ApplyScoreWithAnyValue(Scores score, IPlayer player, IGame game)
        {
            var result = player.GetResultForScore(score);
            if (result == null || result.HasValue) return false;
            game.ApplyScore(result);
            return true;
        }

        private static bool ApplyScoreIfLastRound(Scores score, IPlayer player, IGame game)
        {
            var result = player.GetResultForScore(score);
            if (result == null || result.HasValue || result.PossibleValue <= 0 || player.Roll != 3) return false;
            game.ApplyScore(result);
            return true;
        }

        private static bool ApplyScoreIfHasAllowableValue(Scores score, IPlayer player, IGame game)
        {
            var result = player.GetResultForScore(score);
            if (result == null || result.HasValue || result.PossibleValue < result.MinAllowableValue() ||
                result.PossibleValue <= 0) return false;
            game.ApplyScore(result);
            return true;
        }

        private static bool ApplyScoreIfHasMaxValue(Scores score, IPlayer player, IGame game)
        {
            var result = player.GetResultForScore(score);
            if (result == null || result.HasValue || result.PossibleValue != result.MaxValue ||
                result.PossibleValue <= 0) return false;
            game.ApplyScore(result);
            return true;
        }

        public static void AiDecideRoll(this IPlayer player, IGame game, IDicePanel dicePanel)
        {
            if (game.Rules.CurrentRule == Rules.krMagic && player.MagicalArtifactsForGame.Any())
            {
                var hasFreeHand = Rule.PokerHands.Any(score => !player.GetResultForScore(score).HasValue);
                if (!hasFreeHand) return;
                if (DecideRollOnLastRound(player, game, dicePanel)) return;
                if (DecideRollOnLastRollOfAnyRound(player, game)) return;
            }
            game.ReportRoll();
        }

        private static bool DecideRollOnLastRound(IPlayer player, IGame game, IDicePanel dicePanel)
        {
            if (game.Round != game.Rules.MaxRound) return false;
            if (player.CanUseArtifact(Artifacts.MagicalRoll))
            {
                game.ReportMagicRoll();
                return true;
            }

            if (!player.CanUseArtifact(Artifacts.RollReset))
                return PerformManualChangeOnLastRollOfLastRound(player, game, dicePanel);
            game.ResetRolls();
            return true;
        }

        private static bool PerformManualChangeOnLastRollOfLastRound(IPlayer player, IGame game, IDicePanel dicePanel)
        {
            if (player.Roll != 3) return false;
            if (!player.CanUseArtifact(Artifacts.ManualSet)) return false;
            var hasSmallStraight = player.GetResultForScore(Scores.SmallStraight).HasValue;
            var hasLargeStraight = player.GetResultForScore(Scores.LargeStraight).HasValue;

            var (oldValue, newValue) = game.LastDiceResult
                .AiDecideDiceChange(!hasSmallStraight, !hasLargeStraight);
            var position = dicePanel.GetDicePosition(oldValue);
            if (position == null) return false;
            dicePanel.ManualSetMode = true;
            dicePanel.DieClicked(position.Value);
            dicePanel.ChangeDiceManually(newValue);
            return true;
        }

        private static bool DecideRollOnLastRollOfAnyRound(IPlayer player, IGame game)
        {
            if (player.Roll != 3) return false;
            var numericHands = EnumUtils.GetValues<Scores>().Where(f => f.IsNumeric()).ToList();
            numericHands.Add(Scores.ThreeOfAKind);
            numericHands.Add(Scores.FourOfAKind);
            var areAllNumericFilled = numericHands.Count(score => player.GetResultForScore(score).HasValue)
                                      == numericHands.Count;

            if (!areAllNumericFilled) return true;
            if (player.CanUseArtifact(Artifacts.MagicalRoll))
            {
                game.ReportMagicRoll();
            }

            if (player.CanUseArtifact(Artifacts.RollReset))
            {
                game.ResetRolls();
            }

            return true;

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

        public static List<(int diceValue, int amountOfDice)> AiCalculatesDiceOccurrences(this DieResult diceResult)
        {
            return diceResult.DiceResults.GroupBy(i => i)
                .Select(grp => (grp.Key, grp.Count()))
                .ToList();
        }
    }
}