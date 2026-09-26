using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast that a player changed their dice style.
/// </summary>
public sealed class StyleChangedBroadcast : OnlineMessage
{
    public DiceStyle Style { get; init; }
}
