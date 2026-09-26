using System.Collections.Generic;
using Sanet.MagicalYatzy.Online.Commands.Server.State;

namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast of the final ordered standings when a game finishes.
/// </summary>
public sealed class GameFinishedBroadcast : OnlineMessage
{
    public List<PlayerStanding> Standings { get; init; } = [];
}
