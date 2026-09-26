namespace Sanet.MagicalYatzy.Online.Commands.Client;

/// <summary>
/// Client intent to mark the local player ready (or not) before a game starts.
/// </summary>
public sealed class ReadyCommand : OnlineMessage
{
    public bool IsReady { get; init; }
}
