using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game.Magical;

namespace Sanet.MagicalYatzy.Models.Game
{
    public interface IPlayer
    {
        bool AllNumericFilled { get; }

        IReadOnlyList<RollResult> Results { get; }

        IReadOnlyList<Artifact> MagicalArtifacts { get; }

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

        int Roll { get; }

        int SeatNo { get; }

        int Total { get; }

        int TotalNumeric { get; }

        PlayerType Type { get; }

        void PrepareForGameStart(List<Artifact> availableArtifacts = null);
    }
}
