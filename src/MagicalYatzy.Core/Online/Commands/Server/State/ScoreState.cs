using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Online.Commands.Server.State;

/// <summary>
/// Value-only snapshot of a score entry. <see cref="HasValue"/> distinguishes an unfilled score
/// from a recorded zero.
/// </summary>
public sealed class ScoreState
{
    public Scores ScoreType { get; init; }

    public int Value { get; init; }

    public bool HasValue { get; init; }

    public bool HasBonus { get; init; }
}
