using System.Collections.Generic;

namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// Outcome of a room listing: success carries the matching rooms, failure carries a reason.
/// A successful empty list means no room matched the filter, not that listing failed.
/// </summary>
public sealed record RelayRoomListResult(bool Success, IReadOnlyList<RelayRoomInfo> Rooms, string? Error)
{
    public static RelayRoomListResult Succeeded(IReadOnlyList<RelayRoomInfo> rooms) => new(true, rooms, null);

    public static RelayRoomListResult Failed(string? error) => new(false, [], error);
}