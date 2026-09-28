namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// Result of joining a hosted online game session.
/// </summary>
public sealed record OnlineClientResult(bool Success, string? RoomCode, string? Error)
{
    public static OnlineClientResult Succeeded(string roomCode) =>
        new(true, roomCode, null);

    public static OnlineClientResult Failed(string? roomCode, string error) =>
        new(false, roomCode, error);
}
