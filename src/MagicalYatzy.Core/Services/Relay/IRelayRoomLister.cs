using System.Threading;
using System.Threading.Tasks;

namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// Discovers the relay rooms this app can join. Hides the hub's game-title filter and maps the
/// hub contract onto the local room model, so view models never depend on transport contracts.
/// </summary>
public interface IRelayRoomLister
{
    /// <summary>
    /// Lists the open <see cref="Online.OnlineGameInfo.GameId"/> rooms. An empty
    /// <see cref="RelayRoomListResult.Rooms"/> is a success meaning nothing matched the filter.
    /// </summary>
    Task<RelayRoomListResult> ListRoomsAsync(CancellationToken cancellationToken = default);
}