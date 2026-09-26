namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast of a manual dice change: the current values plus the old/new face values.
/// </summary>
public sealed class DiceChangedBroadcast : OnlineMessage
{
    public int[] Values { get; init; } = [];

    public int OldValue { get; init; }

    public int NewValue { get; init; }

    public bool IsFixed { get; init; }
}
