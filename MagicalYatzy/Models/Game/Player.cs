using Newtonsoft.Json;
using Sanet.MagicalYatzy.Extensions;

namespace Sanet.MagicalYatzy.Models.Game
{
    public class Player: IPlayer
    {
        public bool AllNumericFilled => default;

        public bool CanBuy => default;

        public ClientType Client => default;
        [JsonIgnore]
        public IGame Game { get ; set; }

        public bool HasPassword => default;

        public bool IsBot => default;

        public bool IsDefaultName => default;

        public bool IsHuman => default;
        public bool IsMoving => default;
        public bool IsReady => default;
        public string Language { get; set; }

        public int MaxRemainingNumeric => default;

        public string Name { get;  set; } = "Player 1";
        public string Password { get; set; }
        public string ProfileImage { get; set; } = "SanetDice.png";
        public int Roll { get ; set ; }

        public int SeatNo => default;

        public int Total => default;

        public int TotalNumeric => default;

        public PlayerType Type { get; set; }

        public void Init()
        {

        }

        public override bool Equals(object obj)
        {
            if (!(obj is Player otherPlayer))
                return false;

            return this.Name == otherPlayer.Name && this.Password.Decrypt(33) == otherPlayer.Password.Decrypt(33);
        }

        public override int GetHashCode()
        {
            return $"player{this.Name}{this.Password.Decrypt(33)}".GetHashCode();
        }
    }
}
