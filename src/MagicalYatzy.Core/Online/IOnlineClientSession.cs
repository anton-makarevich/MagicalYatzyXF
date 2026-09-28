using System;
using System.Threading;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Online.Commands.Server;

namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// A guest-side online session: joins a hosted relay room, seats the local player through a
/// <c>JoinGameCommand</c>, hydrates a <see cref="ClientYatzyGame"/> projection from the host's
/// game-state snapshot and applies subsequent broadcasts in order. Local intents flow to the
/// host as commands; only the host changes authoritative state.
/// </summary>
public interface IOnlineClientSession : IAsyncDisposable
{
    /// <summary>The projected game, or <c>null</c> before the first snapshot arrives.</summary>
    ClientYatzyGame? Game { get; }

    /// <summary>The local player's projected seat (with the server-assigned id), or <c>null</c> before a join.</summary>
    IPlayer? LocalPlayer { get; }

    /// <summary>The relay room code, or <c>null</c> before a join / on failure.</summary>
    string? RoomCode { get; }

    /// <summary>
    /// Raised once when the online game ends because the host left or the transport connection
    /// to the host was lost. Raised on a transport thread - a later UI ticket must marshal it.
    /// </summary>
    event Action<GameEndReason>? GameEnded;

    /// <summary>
    /// Joins the relay room for <paramref name="roomCode"/> and seats the local player. Succeeds
    /// once the first game-state snapshot has hydrated the projection; a missing snapshot
    /// (full or absent host) fails after <see cref="OnlineClientSession.JoinTimeout"/>.
    /// </summary>
    Task<OnlineClientResult> JoinAsync(string roomCode, CancellationToken cancellationToken = default);
}
