using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Online.Commands;
using Sanet.MagicalYatzy.Online.Commands.Client;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.Transport;
using Sanet.Transport.SignalR.Client.Relay;

namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// Guest-side online session: mirrors <see cref="OnlineHostSession"/>'s relay lifecycle for the
/// joining client. Owns the relay connection, correlates the join handshake by join-request id,
/// gates the receive pipeline on the first snapshot, applies broadcasts to a
/// <see cref="ClientYatzyGame"/> in arrival order and produces a single reasoned game end when
/// the host departs (graceful broadcast or transport disconnect).
/// </summary>
/// <remarks>
/// Events (<see cref="GameEnded"/> and the projection's <see cref="IGame"/> events) are raised
/// on transport threads - a later UI ticket must marshal them to the UI thread.
/// </remarks>
public sealed class OnlineClientSession : IOnlineClientSession
{
    private readonly IRelayRoomClient _relayRoomClient;
    private readonly IRelayPublisherProvider _publisherProvider;
    private readonly IPlayerService _playerService;
    private readonly CommandTransportAdapter _adapter;
    private readonly SemaphoreSlim _receiveGate = new(1, 1);

    // Publishes are chained so that the command order matches the intent order.
    private readonly Lock _publishLock = new();
    private Task _publishChain = Task.CompletedTask;

    private string? _roomCode;
    private string? _sessionToken;
    private ITransportPublisher? _publisher;
    private string? _joinRequestId;
    private string? _localPlayerId;
    private ClientYatzyGame? _game;
    private TaskCompletionSource<bool>? _joinCompletion;
    private int _gameEndedRaised;
    private bool _isDisposed;

    /// <summary>
    /// How long <see cref="JoinAsync"/> waits for the first game-state snapshot before failing
    /// (the host sends no rejection broadcast for a full or missing room). Default: 30 seconds.
    /// </summary>
    public TimeSpan JoinTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public OnlineClientSession(
        IRelayRoomClient relayRoomClient,
        IRelayPublisherProvider publisherProvider,
        IPlayerService playerService,
        CommandRegistry registry)
    {
        _relayRoomClient = relayRoomClient;
        _publisherProvider = publisherProvider;
        _playerService = playerService;
        _adapter = new CommandTransportAdapter(registry);
    }

    public ClientYatzyGame? Game => _game;

    public IPlayer? LocalPlayer { get; private set; }

    public string? RoomCode => _roomCode;

    /// <inheritdoc />
    public event Action<GameEndReason>? GameEnded;

    #region Join lifecycle

    public async Task<OnlineClientResult> JoinAsync(string roomCode, CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
        {
            return OnlineClientResult.Failed(null, "The session is disposed.");
        }

        var localPlayer = _playerService.CurrentPlayer;
        if (localPlayer == null)
        {
            return OnlineClientResult.Failed(null, "No current player is logged in.");
        }

        _roomCode = roomCode;
        _joinRequestId = Guid.NewGuid().ToString("N");
        _joinCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _adapter.HostDisconnected += OnAdapterHostDisconnected;
        _adapter.Initialize(OnMessageReceived);

        try
        {
            var joinResult = await _relayRoomClient.Join(roomCode, null, cancellationToken);
            if (!joinResult.Success || string.IsNullOrEmpty(joinResult.SessionToken))
            {
                return await CleanupAndFailAsync(
                    joinResult.Error?.Message ?? "Could not join the relay room.");
            }

            _sessionToken = joinResult.SessionToken;

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

            EnqueuePublish(new JoinGameCommand
            {
                PlayerId = _joinRequestId,
                Name = localPlayer.Name
            });

            // The host seats the guest (PlayerJoinedBroadcast with our join request id) and
            // answers with a snapshot; it sends no rejection broadcast for a full or absent seat.
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(JoinTimeout);
            var timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, timeoutCts.Token);
            var completed = await Task.WhenAny(_joinCompletion.Task, timeoutTask);
            if (completed != _joinCompletion.Task)
            {
                // the linked cts fires for both the timeout and the caller's cancellation -
                // only the caller's token turns this into a cancellation
                cancellationToken.ThrowIfCancellationRequested();
                return await CleanupAndFailAsync(
                    "Timed out waiting for the game snapshot from the host.");
            }

            return OnlineClientResult.Succeeded(roomCode);
        }
        catch (OperationCanceledException)
        {
            // cancellation still tears down everything created so far, then propagates
            await CleanupAndFailAsync("Joining was cancelled.");
            throw;
        }
        catch (Exception exception)
        {
            return await CleanupAndFailAsync($"Joining failed unexpectedly: {exception.Message}");
        }
    }

    private async Task<OnlineClientResult> CleanupAndFailAsync(string error)
    {
        _adapter.HostDisconnected -= OnAdapterHostDisconnected;

        if (_publisher != null)
        {
            await _publisher.DisposeAsync();
            _publisher = null;
        }

        // Tear down under the receive gate so no in-flight receive can be mutating the
        // projection while it is detached. The wait is deliberately not cancellable: the
        // cancellation path calls this with an already-cancelled token, and teardown must run.
        await _receiveGate.WaitAsync();
        try
        {
            _adapter.Dispose();
            _game = null;
            LocalPlayer = null;
            _localPlayerId = null;
            _joinRequestId = null;
            _sessionToken = null;
            _joinCompletion = null;
            _publishChain = Task.CompletedTask;

            return OnlineClientResult.Failed(_roomCode, error);
        }
        finally
        {
            _receiveGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        // Best-effort leave of the hosted game, if we are seated.
        if (_localPlayerId != null && _publisher != null)
        {
            try
            {
                await _adapter.PublishMessage(new LeaveGameCommand { PlayerId = _localPlayerId });
            }
            catch
            {
                // best-effort farewell
            }
        }

        _adapter.HostDisconnected -= OnAdapterHostDisconnected;

        if (_publisher != null)
        {
            await _publisher.DisposeAsync();
            _publisher = null;
        }

        // Tear down under the receive gate so no in-flight receive can be mutating the
        // projection while it is detached. _isDisposed already stops new receives from entering.
        await _receiveGate.WaitAsync();
        try
        {
            _adapter.Dispose();
            _sessionToken = null;
            _joinCompletion = null;
            _publishChain = Task.CompletedTask;
        }
        finally
        {
            _receiveGate.Release();
        }
    }

    #endregion

    #region Receive pipeline

    private void OnMessageReceived(OnlineMessage message)
    {
        if (message == null || _isDisposed)
        {
            return;
        }

        _ = ProcessMessageAsync(message);
    }

    private async Task ProcessMessageAsync(OnlineMessage message)
    {
        await _receiveGate.WaitAsync();
        try
        {
            if (_isDisposed)
            {
                return;
            }

            // Join correlation: record the server-assigned id and ask for the snapshot that
            // follows it (the host ignores a state request from an unseated id).
            if (message is PlayerJoinedBroadcast joined
                && joined.JoinRequestId == _joinRequestId
                && _localPlayerId == null)
            {
                _localPlayerId = joined.PlayerId;
                EnqueuePublish(new RequestGameStateCommand { PlayerId = _localPlayerId });
                return;
            }

            if (_game == null)
            {
                // Before the first snapshot only the join handshake matters; other broadcasts
                // (including stale snapshots taken before our seat existed) are dropped - the
                // first snapshot that includes our seat already reflects every earlier change.
                if (message is GameStateBroadcast snapshot
                    && _localPlayerId != null
                    && snapshot.State.Players.Any(p => p.PlayerId == _localPlayerId))
                {
                    _game = new ClientYatzyGame(_localPlayerId, EnqueuePublish);
                    _game.Hydrate(snapshot.State);
                    LocalPlayer = _game.Players.FirstOrDefault(p => p.InGameId == _localPlayerId);
                    _joinCompletion?.TrySetResult(true);
                }

                return;
            }

            switch (message)
            {
                case GameEndedBroadcast ended:
                    OnGameEnded(ended.Reason);
                    break;
                case GameRestartedBroadcast:
                    // resynchronize: the projection is refreshed by the snapshot that follows
                    EnqueuePublish(new RequestGameStateCommand
                    {
                        PlayerId = _localPlayerId ?? string.Empty
                    });
                    break;
                default:
                    _game.ApplyBroadcast(message);
                    break;
            }
        }
        finally
        {
            _receiveGate.Release();
        }
    }

    #endregion

    #region Host departure

    private void OnAdapterHostDisconnected()
    {
        OnGameEnded(GameEndReason.HostDisconnected);
    }

    private void OnGameEnded(GameEndReason reason)
    {
        // The first signal wins; a later signal (broadcast vs transport disconnect) is ignored.
        if (Interlocked.Exchange(ref _gameEndedRaised, 1) == 1)
        {
            return;
        }

        GameEnded?.Invoke(reason);
        _game?.EndGameLocally();
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
                // a failed previous publish must not block later commands
            }

            await adapter.PublishMessage(message).ConfigureAwait(false);
        }
    }

    #endregion
}
