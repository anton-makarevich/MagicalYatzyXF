using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Online.Commands.Server.State;

/// <summary>
/// Value-only snapshot of the authoritative game state.
/// </summary>
public sealed class GameState
{
    public string GameId { get; init; } = string.Empty;

    /// <summary>
    /// The active rule as a <see cref="Rules"/> enum rather than a live <c>Rule</c> object.
    /// </summary>
    public Rules Rule { get; init; }

    public int Round { get; init; }

    public bool IsPlaying { get; init; }

    public bool ReRollMode { get; init; }

    public string CurrentPlayerId { get; init; } = string.Empty;

    public int[] LastDiceValues { get; init; } = [];

    /// <summary>
    /// Multiset of fixed-die face values; duplicates are allowed and preserved.
    /// </summary>
    public int[] FixedDiceValues { get; init; } = [];

    public List<PlayerState> Players { get; init; } = [];
}
