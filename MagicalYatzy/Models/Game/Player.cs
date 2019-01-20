using System;
using System.Collections.Generic;
using System.Linq;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Utils;

namespace Sanet.MagicalYatzy.Models.Game
{
    public class Player: IPlayer
    {
        public Player():this(PlayerType.Local)
        {
        }

        public Player(PlayerType type, IEnumerable<string> playersInGame = null)
        {
            Type = type;

            Name = GetUniqueInGameName(playersInGame?.ToList() ?? new List<string>());
            ProfileImage = (IsBot) ? "BotPlayer.png" : "SanetDice.png";
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

        public int MaxRemainingNumeric => default;

        public string Name { get; set; }
        public string Password { get; set; }
        public string ProfileImage { get; set; }
        public int Roll { get ; set ; }

        public int SeatNo { get; set; }
   
        public int Total => default;
        
        public IReadOnlyList<RollResult> Results { get; private set; }
        
        public IReadOnlyList<Artifact> MagicalArtifactsForGame { get; private set; }
        
        public IReadOnlyList<Artifact> AvailableMagicalArtifacts { get; set; }

        public int TotalNumeric => default;

        public PlayerType Type { get; }
        public string InGameId { get; private set; }
        public DiceStyle SelectedStyle { get; set; }

        #region Methods
        
        public RollResult GetResultForScore(Scores score)=> Results?.FirstOrDefault(f => f.ScoreType == score);

        public void PrepareForGameStart(Rule rule)
        {
            Roll = 1;
            if (rule.CurrentRule == Rules.krMagic)
            {
                MagicalArtifactsForGame = AvailableMagicalArtifacts?.Distinct().ToList();
            }

            Results = rule.ScoresForRule.Select(score => new RollResult(score)).ToList();
            InGameId = Guid.NewGuid().ToString("N");
            IsMyTurn = false;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (!(obj is Player otherPlayer))
                return false;

            return Name == otherPlayer.Name && 
                   Password.Decrypt(33) == otherPlayer.Password.Decrypt(33) &&
                   InGameId == otherPlayer.InGameId ;
        }

        public override int GetHashCode()
        {
            return $"player{this.Name}{this.Password.Decrypt(33)}".GetHashCode();
        }

        private string GetUniqueInGameName(ICollection<string> names)
        {
            var numberOfPlayers = 1;
            var defaultName = (IsBot) ? Strings.BotNameDefault : Strings.PlayerNameDefault;
            do
            {
                var name = $"{defaultName} {numberOfPlayers}";
                if (!names.Contains(name))
                    return name;
                numberOfPlayers++;
            } while (numberOfPlayers < 10000);

            return defaultName;
        }
    }
}
