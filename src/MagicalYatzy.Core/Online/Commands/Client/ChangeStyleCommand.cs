using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Online.Commands.Client;

/// <summary>
/// Client intent to change the local player's dice style.
/// </summary>
public sealed class ChangeStyleCommand : OnlineMessage
{
    public DiceStyle Style { get; init; }
}
