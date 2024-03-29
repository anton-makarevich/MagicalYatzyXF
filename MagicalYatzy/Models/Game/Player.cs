﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game.Ai;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Sanet.MagicalYatzy.Models.Game.Magical;

namespace Sanet.MagicalYatzy.Models.Game
{
    public class Player: IPlayer
    {
        public Player():this(PlayerType.Local)
        {
        }

        public Player(PlayerType type, string playerName = "", string playerImage = "")
        {
            if (playerName == "")
            {
                playerName=type==PlayerType.AI?"Bot":"Unknown";
            }
            Type = type;
            if (type == PlayerType.AI)
                DecisionMaker = new BotDecisionMaker(this);
            Name = playerName;
            ProfileImage = playerImage;
        }

        public bool AllNumericFilled => default;

        public bool CanBuy => default;

        public ClientType Client => default;

        public bool HasPassword => default;

        public bool IsBot => Type == PlayerType.AI;

        public bool IsDefaultName => default;

        public bool IsHuman => Type == PlayerType.Local || Type == PlayerType.Network;
        public bool IsMyTurn { get; set; }

        public bool IsReady { get; set; }

        public string Language { get; set; }

        public int MaxRemainingNumeric => 105-TotalNumeric;

        public string Name { get; set; }
        public string Password { get; set; }
        public string ProfileImage { get; set; }
        public int Roll { get; set; }

        public int SeatNo { get; set; }

        public int Total => (Results == null)
            ? 0 
            : Results.Where(r => r.HasValue).Select(r => r.Value).Sum() + Results.Count(f=>f.HasBonus)*100;

        public IReadOnlyList<IRollResult> Results { get; private set; }
        
        public IReadOnlyList<Artifact> MagicalArtifactsForGame { get; private set; }
        
        public IReadOnlyList<Artifact> AvailableMagicalArtifacts { get; set; }

        public int TotalNumeric => Results?.Where(f => f.IsNumeric && f.HasValue).Sum(f=>f.Value) ?? 0;

        public PlayerType Type { get; }
        public string InGameId { get; private set; }
        public DiceStyle SelectedStyle { get; set; }

        #region Methods
        
        public IRollResult GetResultForScore(Scores score)=> Results?.FirstOrDefault(f => f.ScoreType == score);

        public IGameDecisionMaker DecisionMaker { get; }

        public void PrepareForGameStart(Rule rule)
        {
            Roll = 1;
            if (rule.CurrentRule == Rules.krMagic)
            {
                MagicalArtifactsForGame = AvailableMagicalArtifacts?.Distinct().ToList();
            }

            Results = rule.ScoresForRule.Select(score => new RollResult(score, rule.CurrentRule)).ToList();
            InGameId = Guid.NewGuid().ToString("N");
            IsMyTurn = false;
        }

        public void CheckRollResults(DieResult lastDiceResult, Rule rule)
        {
            foreach (var result in Results)
            {
                if (result.HasValue) continue;
                if (result.IsNumeric)
                    result.PossibleValue = lastDiceResult.YatzyNumberScore((int)result.ScoreType);
                else
                    switch (result.ScoreType)
                    {
                        case Scores.ThreeOfAKind:
                            result.PossibleValue = lastDiceResult.YatzyOfAKindScore(3);
                            break;
                        case Scores.FourOfAKind:
                            result.PossibleValue = lastDiceResult.YatzyOfAKindScore(4);
                            break;
                        case Scores.FullHouse:
                            //if now kniffel extended rules and kniffel has value (0 or 50)
                            if (CheckYatzyJoker(lastDiceResult, rule, result, 25)) break;
                            result.PossibleValue = lastDiceResult.YatzyFullHouseScore();
                            break;
                        case Scores.SmallStraight:
                            if (CheckYatzyJoker(lastDiceResult, rule, result, 30)) break;
                            result.PossibleValue = lastDiceResult.YatzySmallStraightScore();
                            break;
                        case Scores.LargeStraight:
                            if (CheckYatzyJoker(lastDiceResult, rule, result, 40)) break;
                            result.PossibleValue = lastDiceResult.YatzyLargeStraightScore();
                            break;
                        case Scores.Chance:
                            result.PossibleValue = lastDiceResult.YatzyChanceScore();
                            break;
                        case Scores.Kniffel:
                            result.PossibleValue = lastDiceResult.YatzyFiveOfAKindScore();
                            break;
                    }
            }
        }

        public void UseArtifact(Artifacts artifact)
        {
            MagicalArtifactsForGame.FirstOrDefault(f => f.Type == artifact)?.Use();
        }

        private bool CheckYatzyJoker(DieResult lastDiceResult, Rule rule, IRollResult result, int value)
        {
            if (!IsScoreFilled(Scores.Kniffel) || !rule.HasExtendedBonuses ||
                lastDiceResult.YatzyFiveOfAKindScore() != 50) return false;
            //and numeric result corresponded to that kniffel also filled
            var numericResult = GetResultForScore((Scores) lastDiceResult.DiceResults[0]);
            result.PossibleValue = numericResult.HasValue ? value : 0;
            return true;
        }

        public bool IsScoreFilled(Scores score)
        {
            var result =GetResultForScore(score);
            return result != null && result.HasValue;
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (!(obj is Player otherPlayer))
                return false;

            return Name == otherPlayer.Name && 
                   Password.Decrypt(33) == otherPlayer.Password.Decrypt(33) &&
                   InGameId == otherPlayer.InGameId;
        }

        public bool CanUseArtifact(Artifacts artifact)
        {
            var artifactToUse = MagicalArtifactsForGame.FirstOrDefault(f => f.Type == artifact);
            return !artifactToUse?.IsUsed ?? false;
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return $"player{Name}{Password.Decrypt(33)}".GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
    }
}
