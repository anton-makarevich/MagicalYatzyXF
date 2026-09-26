namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast that the turn moved to another player. <see cref="Round"/> mirrors the move
/// order from the game's turn-changed event.
/// </summary>
public sealed class TurnChangedBroadcast : OnlineMessage
{
    public int Round { get; init; }
}
