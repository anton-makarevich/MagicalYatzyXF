namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// Result of starting a hosted online game session.
/// </summary>
public sealed record OnlineHostResult(bool Success, string? RoomCode, string? Error)
{
    public static OnlineHostResult Succeeded(string roomCode) =>
        new(true, roomCode, null);

    public static OnlineHostResult Failed(string? roomCode, string error) =>
        new(false, roomCode, error);
}
