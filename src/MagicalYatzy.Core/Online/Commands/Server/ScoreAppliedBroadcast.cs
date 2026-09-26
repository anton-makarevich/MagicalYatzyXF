using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast that a score was applied to a category.
/// </summary>
public sealed class ScoreAppliedBroadcast : OnlineMessage
{
    public Scores ScoreType { get; init; }

    public int Value { get; init; }

    public bool HasBonus { get; init; }
}
