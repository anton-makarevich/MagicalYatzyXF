using System;
using System.Collections.Generic;
using System.Linq;
using Sanet.MagicalYatzy.Models.Game.Ai.Extensions;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Utils;

namespace Sanet.MagicalYatzy.Models.Game.Ai
{
    public class BotDecisionMaker : IGameDecisionMaker
    {
        private readonly IPlayer _player;

        public BotDecisionMaker(IPlayer player)
        {
            _player = player;
        }

        public bool NeedsToRollAgain()
        {
            var result = _player.GetResultForScore(Scores.Kniffel);
            if (result != null && result.IsMaxPossibleValue)
                return false;

            result = _player.GetResultForScore(Scores.FullHouse);
            if (result != null && result.IsMaxPossibleValue)
                return false;

            result = _player.GetResultForScore(Scores.LargeStraight);
            if (result != null && result.IsMaxPossibleValue)
                return false;

            result = _player.GetResultForScore(Scores.SmallStraight);
            return result == null || !result.IsMaxPossibleValue;
        }

        public void FixDice(IGame game)
        {
            var amountOfDiceForValue = game.LastDiceResult.CalculateDiceOccurrences();

            // check for 3 fives or sixs
            if (FixFivesOrSixs(game, amountOfDiceForValue)) return;

            // check if we need in row values (no large straight) 
            if (FixForLargeStraight(game)) return;

            // check for full house
            if (FixForFullHouse(game, amountOfDiceForValue)) return;

            // fix same if needs kniffel of a kind numerics or chance
            if (FixTheSame(game, amountOfDiceForValue)) return;

            if (_player.IsScoreFilled(Scores.Chance)) return;
            for (var i = 6; i >= 4; i += -1)
            {
                game.FixAllDice(i, true);
            }
        }

        public void DecideFill(IGame game)
        {
            var amountOfDiceForValue = game.LastDiceResult.CalculateDiceOccurrences();

            // check for kniffel
            if (ApplyScoreConsideringCondition(
                Scores.Kniffel,
                game,
                rollResult => rollResult.PossibleValue == rollResult.MaxValue 
                              && rollResult.PossibleValue > 0))
                return;

            // check full house
            if (ApplyScoreConsideringCondition(
                Scores.FullHouse,
                game,
                rollResult => rollResult.PossibleValue == rollResult.MaxValue 
                              && rollResult.PossibleValue > 0)) 
                return;

            // sixs if at least 4 of them and no value 
            if (amountOfDiceForValue.FirstOrDefault(f => f.diceValue == 6).amountOfDice > 3
                && ApplyScoreIfLastRound(Scores.Sixs, game)) return;

            // check for LS
            if (ApplyScoreConsideringCondition(
                Scores.LargeStraight, 
                game,
             rollResult => rollResult.PossibleValue >= rollResult.MinAllowableValue() 
                           && rollResult.PossibleValue > 0)) 
                return;

            //checking  SS and FH
            for (var scoreIndex = 10; scoreIndex >= 9; scoreIndex += -1)
            {
                var index = scoreIndex;
                if (ApplyScoreConsideringCondition(
                    (Scores) scoreIndex,
                    game,
                    rollResult => rollResult.PossibleValue >= rollResult.MinAllowableValue() 
                                        && rollResult.PossibleValue > 0 
                                        && index - _player.Roll < 8))
                    return;
            }

            // 4 and 3 in a row
            for (var scoreIndex = 8; scoreIndex >= 7; scoreIndex += -1)
            {
                if (ApplyScoreConsideringCondition(
                    (Scores) scoreIndex,
                    game,
                    rollResult => _player.Roll == 3
                                  && rollResult.PossibleValue >= rollResult.MinAllowableValue() - (game.Round - 1) / 2
                                  && rollResult.PossibleValue > 0))
                    return;
            }

            // numerics 
            foreach (var (diceValue, _) in amountOfDiceForValue
                .Where(f=>f.diceValue>0)
                .OrderByDescending(f => f.amountOfDice))
            {
                if (ApplyScoreIfLastRound((Scores) diceValue, game)) return;
            }

            // chance
            if (ApplyScoreConsideringCondition(
                Scores.Chance,
                game,
                rollResult => rollResult.PossibleValue > 0
                              && _player.Roll == 3
                              && rollResult.PossibleValue >= rollResult.MinAllowableValue() - (game.Round - 1) / 2))
                return;

            //once again 4 and 3 in row
            for (var scoreIndex = 8; scoreIndex >= 7; scoreIndex += -1)
            {
                if (ApplyScoreIfLastRound((Scores) scoreIndex, game)) return;
            }

            //if not filled - filling at least anything including 0
            for (var scoreIndex = 1; scoreIndex <= 13; scoreIndex++)
            {
                if (ApplyScoreConsideringCondition((Scores) scoreIndex, game)) return;
            }
        }
        
        public void DecideRoll(IGame game, IDicePanel dicePanel)
        {
            if (game.Rules.CurrentRule == Rules.krMagic && _player.MagicalArtifactsForGame.Any())
            {
                var hasFreeHand = Rule.PokerHands.Any(score => !_player.GetResultForScore(score).HasValue);
                if (!hasFreeHand) return;
                if (DecideRollOnLastRound(game, dicePanel)) return;
                if (DecideRollOnLastRollOfAnyRound(game)) return;
            }
            game.ReportRoll();
        }
        
        private bool FixTheSame(IGame game, IEnumerable<(int diceValue, int amountOfDice)> amountOfDiceForValue)
        {
            foreach (var (diceValue, diceAmount) in amountOfDiceForValue.OrderByDescending(f => f.amountOfDice))
            {
                if (diceAmount > 2 | _player.AllNumericFilled)
                {
                    if (_player.IsScoreFilled(Scores.Kniffel)
                        && _player.IsScoreFilled(Scores.ThreeOfAKind)
                        && _player.IsScoreFilled(Scores.FourOfAKind)
                        && _player.IsScoreFilled((Scores) diceValue)) continue;
                    game.FixAllDice(diceValue, true);
                    return true;
                }

                if (_player.IsScoreFilled((Scores) diceValue)) continue;
                game.FixAllDice(diceValue, true);
                return true;
            }

            return false;
        }

        private bool FixForFullHouse(IGame game, IEnumerable<(int diceValue, int amountOfDice)> amountOfDiceForValue)
        {
            var diceToCheck = amountOfDiceForValue.Where(f => f.amountOfDice > 1).ToList();
            var valueTuples = diceToCheck.ToList();
            if (valueTuples.Count() != 2) return false;
            var result = _player.GetResultForScore(Scores.FullHouse);
            if (result == null || result.HasValue) return false;
            foreach (var (diceValue, _) in valueTuples)
            {
                game.FixAllDice(diceValue, true);
            }

            return true;
        }

        private bool FixForLargeStraight(IGame game)
        {
            var result = _player.GetResultForScore(Scores.LargeStraight);
            if (result == null || result.HasValue) return false;
            var (first, count) = game.LastDiceResult.CalculateInRowDice();
            if (count <= 2) return false;
            for (var diceValue = first; diceValue < first + count; diceValue++)
            {
                if (!game.IsDiceFixed(diceValue))
                    game.FixDice(diceValue, true);
            }

            return true;
        }

        private bool FixFivesOrSixs(IGame game, IEnumerable<(int diceValue, int amountOfDice)> amountOfDiceForValue)
        {
            var diceToCheck = amountOfDiceForValue.Where(f => f.diceValue > 4 && f.amountOfDice > 2).ToList();
            foreach (var (diceValue, _) in diceToCheck)
            {
                var result = _player.GetResultForScore((Scores) diceValue);
                if (result == null || result.HasValue) continue;
                game.FixAllDice(diceValue, true);
                return true;
            }

            return false;
        }
        
        private bool ApplyScoreConsideringCondition(
            Scores score, 
            IGame game, 
            Func<IRollResult,bool> additionalCondition = null)
        {
            var result = _player.GetResultForScore(score);
            if (result == null 
                || result.HasValue 
                || additionalCondition != null && !additionalCondition(result)) return false;
            game.ApplyScore(result);
            return true;
        }

        private bool ApplyScoreIfLastRound(Scores score, IGame game)
        {
            return ApplyScoreConsideringCondition(
                score,
                game,
                rollResult => rollResult.PossibleValue > 0 && _player.Roll == 3);
        }
        
        private bool DecideRollOnLastRound(IGame game, IDicePanel dicePanel)
        {
            if (game.Round != game.Rules.MaxRound) return false;
            if (_player.CanUseArtifact(Artifacts.MagicalRoll))
            {
                game.ReportMagicRoll();
                return true;
            }

            if (!_player.CanUseArtifact(Artifacts.RollReset))
                return PerformManualChangeOnLastRollOfLastRound(game, dicePanel);
            game.ResetRolls();
            return true;
        }

        private bool PerformManualChangeOnLastRollOfLastRound(IGame game, IDicePanel dicePanel)
        {
            if (_player.Roll != 3) return false;
            if (!_player.CanUseArtifact(Artifacts.ManualSet)) return false;
            var hasSmallStraight = _player.GetResultForScore(Scores.SmallStraight).HasValue;
            var hasLargeStraight = _player.GetResultForScore(Scores.LargeStraight).HasValue;

            var (oldValue, newValue) = game.LastDiceResult
                .AiDecideDiceChange(!hasSmallStraight, !hasLargeStraight);
            var position = dicePanel.GetDicePosition(oldValue);
            if (position == null) return false;
            dicePanel.ManualSetMode = true;
            dicePanel.DieClicked(position.Value);
            dicePanel.ChangeDiceManually(newValue);
            return true;
        }

        private bool DecideRollOnLastRollOfAnyRound(IGame game)
        {
            if (_player.Roll != 3) return false;
            var numericHands = EnumUtils.GetValues<Scores>().Where(f => f.IsNumeric()).ToList();
            numericHands.Add(Scores.ThreeOfAKind);
            numericHands.Add(Scores.FourOfAKind);
            var areAllNumericFilled = numericHands.Count(score => _player.GetResultForScore(score).HasValue)
                                      == numericHands.Count;

            if (!areAllNumericFilled) return true;
            if (_player.CanUseArtifact(Artifacts.MagicalRoll))
            {
                game.ReportMagicRoll();
            }

            if (_player.CanUseArtifact(Artifacts.RollReset))
            {
                game.ResetRolls();
            }

            return true;
        }
    }
}