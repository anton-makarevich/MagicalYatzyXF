namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast of the dice values after a roll.
/// </summary>
public sealed class DiceRolledBroadcast : OnlineMessage
{
    public int[] Values { get; init; } = [];
}
