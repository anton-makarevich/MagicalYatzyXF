using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Online.Commands.Server.State;

/// <summary>
/// Value-only snapshot of a player in a game-state snapshot.
/// </summary>
public sealed class PlayerState
{
    public string PlayerId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int SeatNo { get; init; }

    public PlayerType Type { get; init; }

    public bool IsReady { get; init; }

    public int Roll { get; init; }

    public DiceStyle SelectedStyle { get; init; }

    public int Total { get; init; }

    public List<ScoreState> Scores { get; init; } = [];

    public List<ArtifactState> Artifacts { get; init; } = [];
}
