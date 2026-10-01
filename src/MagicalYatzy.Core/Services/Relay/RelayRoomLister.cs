using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Online;
using Sanet.Transport.SignalR.Client.Relay;

namespace Sanet.MagicalYatzy.Services.Relay;

/// <inheritdoc cref="IRelayRoomLister" />
public sealed class RelayRoomLister : IRelayRoomLister
{
    private readonly IRelayRoomClient _relayRoomClient;

    public RelayRoomLister(IRelayRoomClient relayRoomClient)
    {
        _relayRoomClient = relayRoomClient;
    }

    public async Task<RelayRoomListResult> ListRoomsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _relayRoomClient.ListRooms(OnlineGameInfo.Filter, cancellationToken);
        if (!result.Success)
            return RelayRoomListResult.Failed(result.Error?.Message);

        return RelayRoomListResult.Succeeded(result.Rooms
            .Select(room => new RelayRoomInfo(
                room.RoomCode,
                room.MemberCount,
                OnlineGameInfo.TryGetRule(room.GameInfo?.Metadata, out var rule) ? rule : null))
            .ToArray());
    }
}