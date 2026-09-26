namespace Sanet.MagicalYatzy.Online.Commands.Client;

/// <summary>
/// Client intent to fix (or unfix) every die showing the given face value.
/// </summary>
public sealed class FixAllDiceCommand : OnlineMessage
{
    public int Value { get; init; }

    public bool IsFixed { get; init; }
}
