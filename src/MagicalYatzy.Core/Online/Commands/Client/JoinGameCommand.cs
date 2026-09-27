namespace Sanet.MagicalYatzy.Online.Commands.Client;

/// <summary>
/// Client intent to join the hosted game and take a seat. <see cref="OnlineMessage.PlayerId"/>
/// carries the caller-generated join request id, which the server echoes back in the resulting
/// <c>PlayerJoinedBroadcast.JoinRequestId</c> so the caller can correlate its own seat.
/// </summary>
public sealed class JoinGameCommand : OnlineMessage
{
    public string Name { get; init; } = string.Empty;
}
