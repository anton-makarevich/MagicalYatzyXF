namespace Sanet.MagicalYatzy.Models.Game
{
    public interface IPlayer
    {
        bool AllNumericFilled { get; }

        bool CanBuy { get; }

        ClientType Client { get; }

        bool HasPassword { get; }

        bool IsBot { get; }
        bool IsDefaultName { get; }

        bool IsHuman { get;  }

        bool IsMoving { get;  }

        bool IsReady { get;  }

        string Language { get; set; }

        int MaxRemainingNumeric { get; }

        string Name { get; set; }

        string Password { get; set; }

        string ProfileImage { get; set; }

        int Roll { get; set; }

        int SeatNo { get; }

        int Total { get; }

        int TotalNumeric { get; }

        PlayerType Type { get; }
    }
}
