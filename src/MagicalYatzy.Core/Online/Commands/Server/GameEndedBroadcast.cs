namespace Sanet.MagicalYatzy.Online.Commands.Server;

/// <summary>
/// Server broadcast that the online game ended for the given reason.
/// </summary>
public sealed class GameEndedBroadcast : OnlineMessage
{
    public GameEndReason Reason { get; init; }
}
