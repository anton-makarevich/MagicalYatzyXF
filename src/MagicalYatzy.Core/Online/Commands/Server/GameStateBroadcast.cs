using Sanet.MagicalYatzy.Online.Commands.Server.State;

namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast carrying a full game-state snapshot.
/// </summary>
public sealed class GameStateBroadcast : OnlineMessage
{
    public GameState State { get; init; } = new();
}
