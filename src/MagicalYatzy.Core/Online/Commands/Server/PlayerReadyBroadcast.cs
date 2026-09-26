namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast that a player's ready state changed.
/// </summary>
public sealed class PlayerReadyBroadcast : OnlineMessage
{
    public bool IsReady { get; init; }
}
