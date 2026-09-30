using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// A relay room this app could join, projected from the hub's <c>RoomSummary</c>.
/// </summary>
/// <param name="RoomCode">Code used to join the room.</param>
/// <param name="MemberCount">Device sessions already in the room; the host counts as one.</param>
/// <param name="Rule">
/// Rule the host created the room with, or <c>null</c> when the room carries no known rule in
/// its metadata - the room is still listed, its rule is just shown as unknown.
/// </param>
public sealed record RelayRoomInfo(string RoomCode, int MemberCount, Rules? Rule);