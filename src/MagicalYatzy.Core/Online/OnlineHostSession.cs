using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Online.Commands;
using Sanet.MagicalYatzy.Online.Commands.Client;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.MagicalYatzy.Online.Commands.Server.State;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.Transport;
using Sanet.Transport.SignalR.Client.Relay;

namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// Host-side online session: owns a <see cref="YatzyServerGame"/>, hosts it in a relay room,
/// seats guests from <see cref="JoinGameCommand"/>s, and bridges every authoritative game event
/// to its matching broadcast. Command dispatch (transport and local) is serialized and guarded
/// by seat-ownership, turn and per-turn-rolled checks.
/// </summary>
public sealed class OnlineHostSession : IOnlineHostSession
{
    private const int MaxPlayers = 4;

    private readonly IRelayRoomClient _relayRoomClient;
    private readonly IRelayPublisherProvider _publisherProvider;
    private readonly IPlayerService _playerService;
    private readonly IDiceGenerator _diceGenerator;
    private readonly CommandTransportAdapter _adapter;
    private readonly SemaphoreSlim _dispatchGate = new(1, 1);

    // Publishes are chained so that the broadcast order matches the game-event order.
    private readonly Lock _publishLock = new();
    private Task _publishChain = Task.CompletedTask;

    // Short-lived join context: consumed by the PlayerJoined handler.
    private string? _pendingJoinRequestId;

    // Short-lived operation context for broadcasts whose fields are absent from event arguments.
    private (int OldValue, int NewValue, bool IsFixed)? _pendingOperation;
    private readonly Lock _contextLock = new();

    private bool _magicRollPending;
    private bool _hasRolled;
    private bool _isDisposed;
    private string? _roomCode;
    private string? _sessionToken;
    private ITransportPublisher? _publisher;
    private YatzyServerGame? _game;

    public OnlineHostSession(
        IRelayRoomClient relayRoomClient,
        IRelayPublisherProvider publisherProvider,
        IPlayerService playerService,
        IDiceGenerator diceGenerator,
        CommandRegistry registry)
    {
        _relayRoomClient = relayRoomClient;
        _publisherProvider = publisherProvider;
        _playerService = playerService;
        _diceGenerator = diceGenerator;
        _adapter = new CommandTransportAdapter(registry);
    }

    public YatzyServerGame? Game => _game;

    public IPlayer? HostPlayer { get; private set; }

    public string? RoomCode => _roomCode;

    #region Hosting lifecycle

    public async Task<OnlineHostResult> HostAsync(Rules rule, CancellationToken cancellationToken = default)
    {
        var hostPlayer = _playerService.CurrentPlayer;
        if (hostPlayer == null)
        {
            return OnlineHostResult.Failed(null, "No current player is logged in.");
        }

        var game = new YatzyServerGame(rule, _diceGenerator);
        _game = game;
        HostPlayer = hostPlayer;

        SubscribeGameEvents();
        game.JoinGame(hostPlayer);
        _adapter.Initialize(OnMessageReceived);

        try
        {
            var createResult = await _relayRoomClient.Create(Guid.Parse(game.GameId), cancellationToken);
            if (!createResult.Success
                || string.IsNullOrEmpty(createResult.RoomCode)
                || createResult.SessionToken == null)
            {
                return await CleanupAndFailAsync(
                    createResult.Error?.Message ?? "Could not create the relay room.");
            }

            var roomCode = createResult.RoomCode;
            _roomCode = roomCode;
            _sessionToken = createResult.SessionToken;

            var ticketResult = await _relayRoomClient.GetRelayTicket(roomCode, _sessionToken, cancellationToken);
            if (!ticketResult.Success || string.IsNullOrEmpty(ticketResult.Ticket))
            {
                return await CleanupAndFailAsync(
                    ticketResult.Error?.Message ?? "Could not obtain a relay ticket.");
            }

            ITransportPublisher publisher;
            try
            {
                publisher = await _publisherProvider.Create(roomCode, ticketResult.Ticket!, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                return await CleanupAndFailAsync(
                    $"Could not connect the relay transport: {exception.Message}");
            }

            _publisher = publisher;
            _adapter.AddPublisher(publisher);

            var readyResult = await _relayRoomClient.Ready(roomCode, _sessionToken, cancellationToken);
            if (!readyResult.Success)
            {
                return await CleanupAndFailAsync(
                    readyResult.Error?.Message ?? "Could not ready the relay room.");
            }

            return OnlineHostResult.Succeeded(roomCode);
        }
        catch (OperationCanceledException)
        {
            // cancellation still tears down everything created so far, then propagates
            await CleanupAndFailAsync("Hosting was cancelled.");
            throw;
        }
        catch (Exception exception)
        {
            return await CleanupAndFailAsync($"Hosting failed unexpectedly: {exception.Message}");
        }
    }

    private async Task<OnlineHostResult> CleanupAndFailAsync(
        string error)
    {
        if (_publisher != null)
        {
            try
            {
                await _adapter.PublishMessage(new GameEndedBroadcast
                {
                    Reason = GameEndReason.HostLeft
                });
            }
            catch
            {
                // best-effort farewell
            }

            await _publisher.DisposeAsync();
            _publisher = null;
        }

        // Tear down under the dispatch gate so no in-flight dispatch can be mutating the game
        // while its events are detached and the game is disposed. The wait is deliberately not
        // cancellable: the cancellation path calls this with an already-cancelled token, and
        // teardown must still run.
        await _dispatchGate.WaitAsync();
        try
        {
            UnsubscribeGameEvents();
            _sessionToken = null;
            HostPlayer = null;
            _game?.Dispose();
            _game = null;
            _adapter.Dispose();

            return OnlineHostResult.Failed(_roomCode, error);
        }
        finally
        {
            _dispatchGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_publisher != null)
        {
            try
            {
                await _adapter.PublishMessage(new GameEndedBroadcast { Reason = GameEndReason.HostLeft });
            }
            catch
            {
                // best-effort farewell
            }

            await _publisher.DisposeAsync();
            _publisher = null;
        }

        // Tear down under the dispatch gate so no in-flight dispatch can be mutating the game
        // while its events are detached and the game is disposed. _isDisposed already stops new
        // dispatches from entering.
        await _dispatchGate.WaitAsync();
        try
        {
            UnsubscribeGameEvents();
            _game?.Dispose();
            _game = null;
            HostPlayer = null;
            _sessionToken = null;
            _adapter.Dispose();
            _publishChain = Task.CompletedTask;
        }
        finally
        {
            _dispatchGate.Release();
        }
    }

    #endregion

    #region Local and transport command entry points

    public void SubmitLocalCommand(OnlineMessage command)
    {
        _ = DispatchAsync(command, isLocal: true);
    }

    /// <summary>
    /// Restarts the authoritative game for every seat, keeping the same roster. The new game is
    /// announced with <see cref="GameRestartedBroadcast"/>, so each client discards its previous
    /// projection and re-synchronises from a fresh snapshot instead of keeping stale scores.
    /// Shares the dispatch gate with commands, so a restart never interleaves with an in-flight
    /// command; no-ops when the session is not hosting.
    /// </summary>
    public async Task RestartGameAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        await _dispatchGate.WaitAsync();
        try
        {
            if (_isDisposed || _game == null)
            {
                return;
            }

            _game.RestartGame();
            EnqueuePublish(new GameRestartedBroadcast());
        }
        finally
        {
            _dispatchGate.Release();
        }
    }

    private void OnMessageReceived(OnlineMessage message)
    {
        _ = DispatchAsync(message, isLocal: false);
    }


    #endregion

    #region Dispatch

    private async Task DispatchAsync(OnlineMessage command, bool isLocal)
    {
        if (command == null || _isDisposed)
        {
            return;
        }

        await _dispatchGate.WaitAsync();
        try
        {
            if (_isDisposed || _game == null)
            {
                return;
            }

            if (command is JoinGameCommand join)
            {
                HandleJoin(join);
                return;
            }

            var player = _game.Players.FirstOrDefault(p => p.InGameId == command.PlayerId);
            if (player == null)
            {
                return;
            }

            if (isLocal)
            {
                if (player != HostPlayer)
                {
                    return;
                }
            }
            else if (player.Type != PlayerType.Network)
            {
                // transport commands are only accepted for network seats
                return;
            }

            switch (command)
            {
                case RequestGameStateCommand:
                    EnqueuePublish(new GameStateBroadcast { PlayerId = player.InGameId, State = GameStateMapper.Map(_game) });
                    return;
                case ReadyCommand ready:
                    _game.SetPlayerReady(player, ready.IsReady);
                    return;
                case ChangeStyleCommand style:
                    _game.ChangeStyle(player, style.Style);
                    return;
                case LeaveGameCommand:
                    _game.LeaveGame(player);
                    return;
            }

            if (!_game.IsPlaying || _game.CurrentPlayer == null || _game.CurrentPlayer.InGameId != player.InGameId)
            {
                return;
            }

            switch (command)
            {
                case RollCommand:
                    _game.ReportRoll();
                    break;
                case MagicRollCommand:
                    _game.ReportMagicRoll();
                    break;
                case ResetRollsCommand:
                    // Rolling back is a magic-rule artifact: the server enforces what the
                    // client UI hides, and consumes the artifact so it cannot be reused.
                    if (_game.Rules.CurrentRule is Rules.krMagic
                        && player.CanUseArtifact(Artifacts.RollReset))
                    {
                        _game.ResetRolls();
                        player.UseArtifact(Artifacts.RollReset);
                    }

                    break;
                case FixDiceCommand fixDice when _hasRolled:
                    _game.FixDice(fixDice.Value, fixDice.IsFixed);
                    break;
                case FixAllDiceCommand fixAll when _hasRolled:
                    _game.FixAllDice(fixAll.Value, fixAll.IsFixed);
                    break;
                case ManualChangeCommand manualChange when _hasRolled:
                    SetOperationContext(manualChange.OldValue, manualChange.NewValue, manualChange.IsFixed);
                    _game.ManualChange(manualChange.OldValue, manualChange.NewValue, manualChange.IsFixed);
                    break;
                case ApplyScoreCommand applyScore when _hasRolled:
                {
                    var result = _game.CurrentPlayer.GetResultForScore(applyScore.ScoreType);
                    if (result != null)
                    {
                        _game.ApplyScore(result);
                    }

                    break;
                }
            }
        }
        finally
        {
            _dispatchGate.Release();
        }
    }

    private void HandleJoin(JoinGameCommand join)
    {
        if (_game.Players.Count >= MaxPlayers)
        {
            return;
        }

        if (string.IsNullOrEmpty(join.PlayerId) || string.IsNullOrEmpty(join.Name))
        {
            return;
        }

        SetPendingJoinRequestId(join.PlayerId);
        _game.JoinGame(new Player(PlayerType.Network, join.Name));

        // The PlayerJoined handler has consumed the pending join request id while
        // JoinGame raised its event; a rejected join never reaches this point and
        // publishes nothing.
        EnqueuePublish(new GameStateBroadcast
        {
            State = GameStateMapper.Map(_game)
        });
    }

    #endregion

    #region Event-to-broadcast bridge

    private void SubscribeGameEvents()
    {
        var events = Game!;
        events.PlayerJoined += OnPlayerJoined;
        events.PlayerLeft += OnPlayerLeft;
        events.PlayerReady += OnPlayerReady;
        events.StyleChanged += OnStyleChanged;
        events.TurnChanged += OnTurnChanged;
        events.DiceFixed += OnDiceFixed;
        events.DiceChanged += OnDiceChanged;
        events.DiceRolled += OnDiceRolled;
        events.PlayerRerolled += OnPlayerRerolled;
        events.MagicRollUsed += OnMagicRollUsed;
        events.ResultApplied += OnResultApplied;
        events.GameFinished += OnGameFinished;
    }

    private void UnsubscribeGameEvents()
    {
        if (_game == null)
        {
            return;
        }

        var events = _game;
        events.PlayerJoined -= OnPlayerJoined;
        events.PlayerLeft -= OnPlayerLeft;
        events.PlayerReady -= OnPlayerReady;
        events.StyleChanged -= OnStyleChanged;
        events.TurnChanged -= OnTurnChanged;
        events.DiceFixed -= OnDiceFixed;
        events.DiceChanged -= OnDiceChanged;
        events.DiceRolled -= OnDiceRolled;
        events.PlayerRerolled -= OnPlayerRerolled;
        events.MagicRollUsed -= OnMagicRollUsed;
        events.ResultApplied -= OnResultApplied;
        events.GameFinished -= OnGameFinished;
    }

    private void OnPlayerJoined(object? sender, PlayerEventArgs e)
    {
        EnqueuePublish(new PlayerJoinedBroadcast
        {
            PlayerId = e.Player.InGameId,
            Name = e.Player.Name,
            SeatNo = e.Player.SeatNo,
            Type = e.Player.Type,
            JoinRequestId = TakePendingJoinRequestId()
        });
    }

    private void OnPlayerLeft(object? sender, PlayerEventArgs e)
    {
        EnqueuePublish(new PlayerLeftBroadcast { PlayerId = e.Player.InGameId });
    }

    private void OnPlayerReady(object? sender, PlayerEventArgs e)
    {
        EnqueuePublish(new PlayerReadyBroadcast
        {
            PlayerId = e.Player.InGameId,
            IsReady = e.Player.IsReady
        });
    }

    private void OnStyleChanged(object? sender, PlayerEventArgs e)
    {
        EnqueuePublish(new StyleChangedBroadcast
        {
            PlayerId = e.Player.InGameId,
            Style = e.Player.SelectedStyle
        });
    }

    private void OnTurnChanged(object? sender, MoveEventArgs e)
    {
        _hasRolled = false;
        lock (_contextLock)
        {
            _pendingOperation = null;
        }

        _magicRollPending = false;
        EnqueuePublish(new TurnChangedBroadcast
        {
            PlayerId = e.Player.InGameId,
            Round = e.Move
        });
    }

    private void OnDiceFixed(object? sender, FixDiceEventArgs e)
    {
        EnqueuePublish(new DiceFixedBroadcast
        {
            PlayerId = e.Player?.InGameId,
            Value = e.Value,
            IsFixed = e.Isfixed,
            All = false
        });
    }

    private void OnDiceChanged(object? sender, RollEventArgs e)
    {
        e.Player?.CheckRollResults(
            new DieResult { DiceResults = [.. e.Value] },
            Game.Rules);

        var (oldValue, newValue, isFixed) = TakeOperationContext();
        EnqueuePublish(new DiceChangedBroadcast
        {
            PlayerId = e.Player?.InGameId,
            Values = e.Value,
            OldValue = oldValue,
            NewValue = newValue,
            IsFixed = isFixed
        });
    }

    private void OnDiceRolled(object? sender, RollEventArgs e)
    {
        _hasRolled = true;
        e.Player?.CheckRollResults(
            new DieResult { DiceResults = [.. e.Value] },
            Game.Rules);

        if (_magicRollPending)
        {
            _magicRollPending = false;
            EnqueuePublish(new MagicRollUsedBroadcast
            {
                PlayerId = e.Player?.InGameId,
                Values = e.Value
            });
        }

        EnqueuePublish(new DiceRolledBroadcast
        {
            PlayerId = e.Player?.InGameId,
            Values = e.Value
        });
    }

    private void OnPlayerRerolled(object? sender, PlayerEventArgs e)
    {
        EnqueuePublish(new PlayerRerolledBroadcast { PlayerId = e.Player.InGameId });
    }

    private void OnMagicRollUsed(object? sender, PlayerEventArgs e)
    {
        _magicRollPending = true;
    }

    private void OnResultApplied(object? sender, RollResultEventArgs e)
    {
        EnqueuePublish(new ScoreAppliedBroadcast
        {
            PlayerId = e.Player.InGameId,
            ScoreType = e.ScoreType,
            Value = e.Value,
            HasBonus = e.HasBonus
        });
    }

    private void OnGameFinished(object? sender, EventArgs e)
    {
        EnqueuePublish(new GameFinishedBroadcast
        {
            Standings =
            [
                .. Game.Players
                    .OrderByDescending(p => p.Total)
                    .Select(p => new PlayerStanding
                    {
                        PlayerId = p.InGameId,
                        Total = p.Total
                    })
            ]
        });
    }

    #endregion

    #region Publishing

    private void EnqueuePublish(OnlineMessage message)
    {
        Task previous;
        Task publishTask;
        lock (_publishLock)
        {
            previous = _publishChain;
            _publishChain = publishTask = PublishAfterAsync(previous, _adapter, message);
        }

        _ = publishTask;

        static async Task PublishAfterAsync(Task previous, CommandTransportAdapter adapter, OnlineMessage message)
        {
            try
            {
                await previous.ConfigureAwait(false);
            }
            catch
            {
                // a failed previous publish must not block later broadcasts
            }

            await adapter.PublishMessage(message).ConfigureAwait(false);
        }
    }

    #endregion

    #region Join/operation context helpers

    private void SetPendingJoinRequestId(string joinRequestId)
    {
        lock (_contextLock)
        {
            _pendingJoinRequestId = joinRequestId;
        }
    }

    private string? TakePendingJoinRequestId()
    {
        lock (_contextLock)
        {
            var value = _pendingJoinRequestId;
            _pendingJoinRequestId = null;
            return value;
        }
    }

    private void SetOperationContext(int oldValue, int newValue, bool isFixed)
    {
        lock (_contextLock)
        {
            _pendingOperation = (oldValue, newValue, isFixed);
        }
    }

    private (int OldValue, int NewValue, bool IsFixed) TakeOperationContext()
    {
        lock (_contextLock)
        {
            var value = _pendingOperation;
            _pendingOperation = null;
            return value ?? (0, 0, false);
        }
    }

    #endregion
}
