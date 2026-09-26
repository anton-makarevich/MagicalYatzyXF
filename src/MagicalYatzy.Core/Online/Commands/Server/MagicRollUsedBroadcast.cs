namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast that a player used the magical roll artifact.
/// </summary>
public sealed class MagicRollUsedBroadcast : OnlineMessage
{
    public int[] Values { get; init; } = [];
}
