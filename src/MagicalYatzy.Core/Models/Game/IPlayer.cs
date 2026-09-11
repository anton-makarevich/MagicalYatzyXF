using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game.Ai;
using Sanet.MagicalYatzy.Models.Game.Magical;

namespace Sanet.MagicalYatzy.Models.Game
{
    public interface IPlayer
    {
        bool AllNumericFilled { get; }

        IReadOnlyList<IRollResult> Results { get; }

        IReadOnlyList<Artifact> MagicalArtifactsForGame { get; }
        
        IReadOnlyList<Artifact> AvailableMagicalArtifacts { get; set; }

        bool CanBuy { get; }

        ClientType Client { get; }

        bool HasPassword { get; }

        bool IsBot { get; }
        bool IsDefaultName { get; }

        bool IsHuman { get;  }

        bool IsMyTurn { get; set; }

        bool IsReady { get; set; }

        string Language { get; set; }

        int MaxRemainingNumeric { get; }

        string Name { get; set; }

        string Password { get; set; }

        string ProfileImage { get; set; }

        int Roll { get; set; }

        int SeatNo { get; set; }

        int Total { get; }

        int TotalNumeric { get; }

        PlayerType Type { get; }
        string InGameId { get; }
        DiceStyle SelectedStyle { get; set; }
        IGameDecisionMaker DecisionMaker { get; }

        void PrepareForGameStart(Rule rule);
        
        IRollResult GetResultForScore(Scores score);

        void CheckRollResults(DieResult lastDiceResult, Rule rule);
        void UseArtifact(Artifacts artifact);

        bool IsScoreFilled(Scores score);

        bool CanUseArtifact(Artifacts artifact);
    }
}
