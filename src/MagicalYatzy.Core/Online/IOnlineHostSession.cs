using System;
using System.Threading;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Online.Commands;

namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// A host-side online session: owns the authoritative game, hosts it in a relay room, seats
/// guests and broadcasts every authoritative game event. Host intents go through
/// <see cref="SubmitLocalCommand"/> so they share the same guards as guest commands.
/// </summary>
public interface IOnlineHostSession : IAsyncDisposable
{
    /// <summary>The authoritative game owned by this session, or <c>null</c> before hosting.</summary>
    YatzyServerGame? Game { get; }

    /// <summary>The host player, or <c>null</c> before hosting.</summary>
    IPlayer? HostPlayer { get; }

    /// <summary>The relay room code, or <c>null</c> before the room is created / on failure.</summary>
    string? RoomCode { get; }

    /// <summary>
    /// Creates the authoritative game and the relay room lifecycle for the given rule.
    /// </summary>
    Task<OnlineHostResult> HostAsync(Rules rule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Local entry point for host-player commands. Applies the same seat, turn and rolled
    /// guards as transport commands; targets the host seat only.
    /// </summary>
    void SubmitLocalCommand(OnlineMessage command);

    /// <summary>
    /// Restarts the authoritative game for the same roster and tells every client to
    /// re-synchronise from a fresh snapshot. No-op before hosting starts or after disposal.
    /// </summary>
    Task RestartGameAsync();
}
