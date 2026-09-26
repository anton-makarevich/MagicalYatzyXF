namespace Sanet.MagicalYatzy.Online.Commands.Client;

/// <summary>
/// Client intent to manually change a die from one face value to another.
/// </summary>
public sealed class ManualChangeCommand : OnlineMessage
{
    public int OldValue { get; init; }

    public int NewValue { get; init; }

    public bool IsFixed { get; init; }
}
