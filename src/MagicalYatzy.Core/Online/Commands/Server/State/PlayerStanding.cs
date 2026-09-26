namespace Sanet.MagicalYatzy.Online.Commands.Server.State;

/// <summary>
/// A single entry in the final game standings: a player and their total.
/// </summary>
public sealed class PlayerStanding
{
    public string PlayerId { get; init; } = string.Empty;

    public int Total { get; init; }
}
