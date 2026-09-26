namespace Sanet.MagicalYatzy.Online.Commands.Client;

/// <summary>
/// Client intent to fix (or unfix) a single die with the given face value.
/// </summary>
public sealed class FixDiceCommand : OnlineMessage
{
    public int Value { get; init; }

    public bool IsFixed { get; init; }
}
