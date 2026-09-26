namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast that a die (or all dice) was fixed or unfixed.
/// </summary>
public sealed class DiceFixedBroadcast : OnlineMessage
{
    public int Value { get; init; }

    public bool IsFixed { get; init; }

    /// <summary>
    /// True when the broadcast covers a fix-all-dice action rather than a single die.
    /// </summary>
    public bool All { get; init; }
}
