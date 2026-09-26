using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast that a player joined the game.
/// </summary>
public sealed class PlayerJoinedBroadcast : OnlineMessage
{
    public string Name { get; init; } = string.Empty;

    public int SeatNo { get; init; }

    public PlayerType Type { get; init; }
}
