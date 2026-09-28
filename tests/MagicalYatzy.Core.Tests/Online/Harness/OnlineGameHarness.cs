using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MagicalYatzy.Core.Tests.Online.Fakes;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Online;
using Sanet.MagicalYatzy.Online.Commands.Client;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.Transport;
using Sanet.Transport.SignalR.Client.Relay;
using Shouldly;

namespace MagicalYatzy.Core.Tests.Online.Harness;

/// <summary>
/// One completed turn as the harness drove it, so a test can assert what the host committed
/// after the fact.
/// </summary>
public sealed record TurnRecord(string PlayerId, IReadOnlyList<int> Dice, int? FixedValue, Scores ScoreType, int ScoreValue);

/// <summary>
/// Records the end-of-game signals a guest session raises. The session only reports the first
/// one, so a test asserts the whole list to prove a later signal was ignored.
/// </summary>
public sealed class SessionEventRecorder
{
    private readonly Lock _syncLock = new();
    private readonly List<GameEndReason> _gameEndedReasons = [];
    private TaskCompletionSource<GameEndReason> _firstGameEnded = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IReadOnlyList<GameEndReason> GameEndedReasons
    {
        get
        {
            lock (_syncLock)
            {
                return _gameEndedReasons.ToList();
            }
        }
    }

    public static SessionEventRecorder Attach(OnlineClientSession session)
    {
        var recorder = new SessionEventRecorder();
        session.GameEnded += recorder.OnGameEnded;
        return recorder;
    }

    /// <summary>
    /// Waits for the first end signal and returns its reason, or null when none arrives in time.
    /// </summary>
    public async Task<GameEndReason?> WaitForFirstGameEndAsync(TimeSpan timeout)
    {
        try
        {
            return await _firstGameEnded.Task.WaitAsync(timeout);
        }
        catch (TimeoutException)
        {
            return null;
        }
    }

    private void OnGameEnded(GameEndReason reason)
    {
        lock (_syncLock)
        {
            _gameEndedReasons.Add(reason);
        }

        _firstGameEnded.TrySetResult(reason);
    }
}

/// <summary>
/// Drives a real online game end to end: one <see cref="OnlineHostSession"/> and up to three
/// <see cref="OnlineClientSession"/> guests over an in-memory relay room. Commands are submitted
/// through the same intent APIs the view models use, so a test covers the whole
/// intent-broadcast-projection chain instead of single sessions in isolation.
/// </summary>
public sealed class OnlineGameHarness : IAsyncDisposable
{
    public const string RoomCode = "ABC123";

    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(10);
    private static readonly int[] DefaultDice = [1, 2, 3, 4, 5];

    private readonly FakeRelayRoom _room = new();
    private readonly IRelayRoomClient _relayRoomClient = Substitute.For<IRelayRoomClient>();
    private readonly IRelayPublisherProvider _publisherProvider = Substitute.For<IRelayPublisherProvider>();
    private readonly IPlayerService _hostPlayerService = Substitute.For<IPlayerService>();
    private readonly IDiceGenerator _diceGenerator = Substitute.For<IDiceGenerator>();
    private readonly List<OnlineClientSession> _guests = [];
    private readonly List<FakeTransportPublisher> _createdPublishers = [];
    private readonly Dictionary<OnlineClientSession, FakeTransportPublisher> _publishers = [];
    private readonly Dictionary<OnlineClientSession, GameEventRecorder> _gameEvents = [];
    private readonly Dictionary<OnlineClientSession, SessionEventRecorder> _sessionEvents = [];
    private bool _isDisposed;

    private OnlineGameHarness()
    {
    }

    public OnlineHostSession Host { get; private set; } = null!;

    /// <summary>The guests that are still connected.</summary>
    public IReadOnlyList<OnlineClientSession> Guests => _guests;

    public YatzyServerGame HostGame => Host.Game!;

    public IPlayer HostPlayer => Host.HostPlayer!;

    public string HostPlayerId => HostPlayer.InGameId!;

    public string CurrentPlayerId => HostGame.CurrentPlayer!.InGameId!;

    /// <summary>
    /// Hosts a game and seats <paramref name="playerCount"/> players in total: the host plus
    /// <paramref name="playerCount"/> - 1 guests that join one after another.
    /// </summary>
    public static async Task<OnlineGameHarness> StartAsync(int playerCount, Rules rules = Rules.krExtended)
    {
        if (playerCount is < 2 or > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(playerCount), playerCount, "an online game seats 2 to 4 players");
        }

        var harness = new OnlineGameHarness();
        harness.SetupRelayCalls();
        harness.SetupDeterministicDice(DefaultDice);
        harness._hostPlayerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        harness.Host = harness.CreateHostSession();

        var hostResult = await harness.Host.HostAsync(rules);
        hostResult.Success.ShouldBeTrue(hostResult.Error ?? "hosting failed");

        for (var seat = 1; seat < playerCount; seat++)
        {
            await harness.JoinGuestAsync($"Guest{seat}");
        }

        return harness;
    }

    public OnlineClientSession? GuestFor(string playerId) =>
        _guests.SingleOrDefault(g => g.LocalPlayer?.InGameId == playerId);

    public FakeTransportPublisher PublisherFor(OnlineClientSession guest) => _publishers[guest];

    public GameEventRecorder GameEvents(OnlineClientSession guest) => _gameEvents[guest];

    public SessionEventRecorder SessionEvents(OnlineClientSession guest) => _sessionEvents[guest];

    /// <summary>
    /// Lets pending work and timers run for a moment, so a test can prove something never happens.
    /// </summary>
    public Task SettleAsync() => Task.Delay(200);

    /// <summary>Marks every seat ready, which starts the game once the last one reports in.</summary>
    public void ReadyAll()
    {
        Host.SubmitLocalCommand(new ReadyCommand { PlayerId = HostPlayerId, IsReady = true });
        foreach (var guest in _guests)
        {
            guest.Game!.SetPlayerReady(guest.LocalPlayer!, true);
        }
    }

    public Task WaitForPlayingAsync() => WaitFor(
        () => HostGame.IsPlaying && Guests.All(g => g.Game!.IsPlaying && g.Game.CurrentPlayer != null),
        "the game never started on every session");

    public Task WaitForTurnOnAsync(string playerId) => WaitFor(
        () => HostGame.CurrentPlayer?.InGameId == playerId
            && Guests.All(g => g.Game!.CurrentPlayer?.InGameId == playerId),
        $"the turn never reached {playerId} on every projection");

    public Task WaitForAppliedAsync(string playerId, Scores scoreType) => WaitFor(() =>
    {
        var hostResult = HostGame.Players.FirstOrDefault(p => p.InGameId == playerId)
            ?.GetResultForScore(scoreType);
        return hostResult?.HasValue == true
            && Guests.All(g => g.Game!.Players
                .FirstOrDefault(p => p.InGameId == playerId)
                ?.GetResultForScore(scoreType)?.HasValue == true);
    }, $"the score {scoreType} of {playerId} was never committed on every projection");

    /// <summary>
    /// Waits until the turn host and every guest projection agree on the turn, round and roster.
    /// </summary>
    public Task WaitForProjectionsAgreeAsync() => WaitFor(
        () => ProjectionMismatch() == null,
        ProjectionMismatch,
        "the guest projections never agreed with the host");

    /// <summary>
    /// Describes the first difference between a guest projection and the host game, or null when
    /// they agree - so a failing wait says what actually diverged.
    /// Seat numbers are deliberately not compared: the host owns seat ordering, renumbers it on
    /// round boundaries and broadcasts seat changes only in snapshots, while a projection just
    /// carries the seats it last hydrated.
    /// </summary>
    private string? ProjectionMismatch()
    {
        foreach (var guest in Guests)
        {
            var game = guest.Game!;
            var name = guest.LocalPlayer!.Name;
            if (game.IsPlaying != HostGame.IsPlaying)
            {
                return $"{name}: playing {game.IsPlaying} instead of {HostGame.IsPlaying}";
            }

            if (game.Round != HostGame.Round)
            {
                return $"{name}: round {game.Round} instead of {HostGame.Round}";
            }

            if (game.CurrentPlayer?.InGameId != HostGame.CurrentPlayer?.InGameId)
            {
                return $"{name}: turn {game.CurrentPlayer?.InGameId} instead of {HostGame.CurrentPlayer?.InGameId}";
            }

            if (!game.Players.Select(p => p.InGameId).SequenceEqual(HostGame.Players.Select(p => p.InGameId)))
            {
                return $"{name}: roster [{string.Join(", ", game.Players.Select(p => p.InGameId))}]"
                    + $" instead of [{string.Join(", ", HostGame.Players.Select(p => p.InGameId))}]";
            }
        }

        return null;
    }

    /// <summary>Rolls on behalf of the seat that owns the current turn.</summary>
    public async Task<IReadOnlyList<int>> RollAsync()
    {
        var playerId = CurrentPlayerId;
        await WaitForTurnOnAsync(playerId);
        if (GuestFor(playerId) is { } guest)
        {
            guest.Game!.ReportRoll();
        }
        else
        {
            Host.SubmitLocalCommand(new RollCommand { PlayerId = playerId });
        }

        await WaitFor(() => HostGame.LastDiceResult.DiceResults.Count == DefaultDice.Length
            && Guests.All(g => g.Game!.LastDiceResult.DiceResults.Count == DefaultDice.Length),
            $"the roll of {playerId} never reached every projection");
        return HostGame.LastDiceResult.DiceResults.ToList();
    }

    /// <summary>Fixes one die of the current roll, the way a player's view model would.</summary>
    public async Task FixDiceAsync(int value)
    {
        var playerId = CurrentPlayerId;
        if (GuestFor(playerId) is { } guest)
        {
            guest.Game!.FixDice(value, true);
        }
        else
        {
            HostGame.FixDice(value, true);
        }

        await WaitFor(() => HostGame.FixedRollResults.Contains(value)
            && HostGame.NumberOfFixedDice == 1
            && Guests.All(g => g.Game!.FixedRollResults.Contains(value) && g.Game.NumberOfFixedDice == 1),
            $"the fixed die {value} of {playerId} never reached every projection");
    }

    /// <summary>Applies a score on behalf of the seat that owns the current turn.</summary>
    public async Task ApplyScoreAsync(Scores scoreType)
    {
        var playerId = CurrentPlayerId;
        if (GuestFor(playerId) is { } guest)
        {
            guest.Game!.ApplyScore(new RollResult(scoreType, HostGame.Rules.CurrentRule));
        }
        else
        {
            Host.SubmitLocalCommand(new ApplyScoreCommand { PlayerId = playerId, ScoreType = scoreType });
        }

        await WaitForAppliedAsync(playerId, scoreType);
    }

    /// <summary>
    /// Plays the current turn to its end: roll, keep one die, then score the first open entry.
    /// </summary>
    public async Task<TurnRecord> DriveTurnAsync(bool fixDie = true)
    {
        var playerId = CurrentPlayerId;
        var dice = await RollAsync();
        int? fixedValue = null;
        if (fixDie)
        {
            fixedValue = dice[0];
            await FixDiceAsync(fixedValue.Value);
        }

        var scoreType = FirstOpenScore(HostGame.CurrentPlayer!);
        await ApplyScoreAsync(scoreType);
        var scoreValue = HostGame.Players
            .Single(p => p.InGameId == playerId)
            .GetResultForScore(scoreType)!.Value;
        await WaitForProjectionsAgreeAsync();
        return new TurnRecord(playerId, dice, fixedValue, scoreType, scoreValue);
    }

    /// <summary>Plays every remaining turn and waits for the game to finish everywhere.</summary>
    public async Task PlayToCompletionAsync(int maxTurns = 250)
    {
        var turns = 0;
        while (HostGame.IsPlaying)
        {
            if (turns++ >= maxTurns)
            {
                throw new ShouldAssertException(
                    $"the game was still playing after {maxTurns} turns, sitting at round {HostGame.Round}");
            }

            await DriveTurnAsync();
        }

        await WaitFor(() => Guests.All(g => !g.Game!.IsPlaying), "the guests never saw the game end");
        await WaitForProjectionsAgreeAsync();
    }

    /// <summary>Restarts on the host and waits until every guest rebuilt its projection.</summary>
    public async Task RestartAsync()
    {
        await Host.RestartGameAsync();
        await WaitFor(() => HostGame.Round == 1
            && !HostGame.IsPlaying
            && Guests.All(g => g.Game!.Round == 1
                && !g.Game.IsPlaying
                && g.Game.Players.Select(p => p.InGameId)
                    .SequenceEqual(HostGame.Players.Select(p => p.InGameId))
                && g.Game.Players.Select(p => p.SeatNo)
                    .SequenceEqual(HostGame.Players.Select(p => p.SeatNo))
                && g.Game.Players.SelectMany(p => p.Results!).All(r => !r.HasValue)),
            "the guests never rebuilt their projection for the restarted game");
    }

    /// <summary>
    /// Disposes a guest session, as quitting the app would, and waits until the host seats it out.
    /// </summary>
    public async Task LeaveAsync(OnlineClientSession guest)
    {
        var playerId = guest.LocalPlayer!.InGameId!;
        _guests.Remove(guest);
        _publishers.Remove(guest);
        _gameEvents.Remove(guest);
        _sessionEvents.Remove(guest);
        await guest.DisposeAsync();
        await WaitFor(() => HostGame.Players.All(p => p.InGameId != playerId),
            $"the host never seated out the guest {playerId}");
    }

    /// <summary>
    /// Asserts that every guest projection mirrors the host game: roster, score sheets, totals,
    /// bonuses, the current turn and the round. Seat numbers are host-owned and are only mirrored
    /// when a projection hydrates, so they are not asserted here.
    /// </summary>
    public void AssertProjectionsMatchHost()
    {
        foreach (var guest in Guests)
        {
            var game = guest.Game!;
            game.Round.ShouldBe(HostGame.Round, $"round mismatch for {guest.LocalPlayer!.Name}");
            game.IsPlaying.ShouldBe(HostGame.IsPlaying, $"playing state mismatch for {guest.LocalPlayer!.Name}");
            game.CurrentPlayer?.InGameId.ShouldBe(HostGame.CurrentPlayer?.InGameId);
            game.Players.Select(p => p.InGameId).ShouldBe(HostGame.Players.Select(p => p.InGameId));
            game.Players.Select(p => p.Total).ShouldBe(HostGame.Players.Select(p => p.Total));
            game.Players.Select(p => p.TotalNumeric).ShouldBe(HostGame.Players.Select(p => p.TotalNumeric));
            game.Players.Select(p => BonusValue(p)).ShouldBe(HostGame.Players.Select(BonusValue));

            for (var seat = 0; seat < game.Players.Count; seat++)
            {
                var projected = game.Players[seat].Results!;
                var hosted = HostGame.Players[seat].Results!;
                projected.Select(r => r.ScoreType).ShouldBe(hosted.Select(r => r.ScoreType));
                projected.Select(r => r.HasValue).ShouldBe(hosted.Select(r => r.HasValue));
                projected.Select(r => r.HasBonus).ShouldBe(hosted.Select(r => r.HasBonus));
                projected.Select(r => r.Value).ShouldBe(hosted.Select(r => r.Value));
            }
        }
    }

    /// <summary>
    /// The first open non-Bonus entry of a sheet, the same one the round timeout auto-fills.
    /// </summary>
    public static Scores FirstOpenScore(IPlayer player) => player.Results!
        .First(r => r.ScoreType != Scores.Bonus && !r.HasValue).ScoreType;

    private static int BonusValue(IPlayer player) => player.GetResultForScore(Scores.Bonus)!.Value;

    public ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return ValueTask.CompletedTask;
        }

        _isDisposed = true;
        return new ValueTask(DisposeSessionsAsync());
    }

    private OnlineHostSession CreateHostSession() => new(
        _relayRoomClient,
        _publisherProvider,
        _hostPlayerService,
        _diceGenerator,
        new CommandRegistry());

    private void SetupRelayCalls()
    {
        _relayRoomClient.Create(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(RoomSessionResult.Succeeded(
                RoomCode, "host-session-token", "Host", Guid.NewGuid(), Guid.NewGuid())));
        _relayRoomClient.Join(RoomCode, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(RoomSessionResult.Succeeded(
                RoomCode, "guest-session-token", "Guest", Guid.NewGuid(), Guid.NewGuid())));
        _relayRoomClient.GetRelayTicket(RoomCode, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(RelayTicketResult.Succeeded(
                "relay-ticket", DateTimeOffset.UtcNow.AddMinutes(10))));
        _relayRoomClient.Ready(RoomCode, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(RoomOperationResult.Succeeded()));
        _publisherProvider.Create(RoomCode, "relay-ticket", Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                var publisher = new FakeTransportPublisher();
                _createdPublishers.Add(publisher);
                return Task.FromResult<ITransportPublisher>(_room.Join(publisher));
            });
    }

    private void SetupDeterministicDice(params int[] values)
    {
        var queue = new Queue<int>(values);
        _diceGenerator.GetNextDiceResult(Arg.Any<int[]>())
            .Returns(_ => queue.Count == 0 ? values[^1] : queue.Dequeue());
    }

    private async Task JoinGuestAsync(string playerName)
    {
        var playerService = Substitute.For<IPlayerService>();
        playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, playerName));
        var guest = new OnlineClientSession(
            _relayRoomClient,
            _publisherProvider,
            playerService,
            new CommandRegistry());

        var result = await guest.JoinAsync(RoomCode);
        result.Success.ShouldBeTrue(result.Error ?? $"{playerName} failed to join");
        // the relay created exactly one publisher for this join, so it is the newest one
        _publishers[guest] = _createdPublishers[^1];
        _guests.Add(guest);
        // hydration raises no gameplay events, so the recorder has to attach right after the join
        _gameEvents[guest] = GameEventRecorder.Attach(guest.Game!);
        _sessionEvents[guest] = SessionEventRecorder.Attach(guest);
    }

    private static Task WaitFor(Func<bool> condition, string message) =>
        WaitFor(condition, () => null, message);

    /// <summary>
    /// Polls <paramref name="condition"/> until it holds and, when it never does, reports the last
    /// <paramref name="detail"/> so the failure says what was still outstanding.
    /// </summary>
    private static async Task WaitFor(Func<bool> condition, Func<string?> detail, string message)
    {
        var limit = DateTime.UtcNow + WaitTimeout;
        while (DateTime.UtcNow < limit)
        {
            try
            {
                if (condition())
                {
                    return;
                }
            }
            catch (InvalidOperationException)
            {
                // a collection can be replaced while a broadcast is applied on another thread
            }

            await Task.Delay(PollInterval);
        }

        var last = detail();
        condition().ShouldBeTrue(last == null ? message : $"{message}: {last}");
    }

    private async Task DisposeSessionsAsync()
    {
        foreach (var guest in _guests)
        {
            await guest.DisposeAsync();
        }

        _guests.Clear();
        await Host.DisposeAsync();
    }
}
