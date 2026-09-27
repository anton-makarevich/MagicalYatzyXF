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
using Sanet.MagicalYatzy.Online.Commands;
using Sanet.MagicalYatzy.Online.Commands.Client;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.Transport;
using Sanet.Transport.SignalR.Client.Relay;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.Online;

public class OnlineHostSessionTests
{
    private static readonly TimeSpan SettleDelay = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

    private readonly FakeRelayRoom _room = new();
    private readonly IRelayRoomClient _relayRoomClient = Substitute.For<IRelayRoomClient>();
    private readonly IRelayPublisherProvider _publisherProvider = Substitute.For<IRelayPublisherProvider>();
    private readonly IPlayerService _playerService = Substitute.For<IPlayerService>();
    private readonly IDiceGenerator _diceGenerator = Substitute.For<IDiceGenerator>();

    private sealed class Guest
    {
        public FakeTransportPublisher Publisher { get; } = new();

        public CommandTransportAdapter Adapter { get; } = new(new CommandRegistry());

        public List<OnlineMessage> Received { get; } = [];

        public void Attach(FakeRelayRoom room)
        {
            room.Join(Publisher);
            Adapter.AddPublisher(Publisher);
            Adapter.Initialize(Received.Add);
        }

        public Task Send(OnlineMessage command) => Adapter.PublishMessage(command);

        public IReadOnlyList<T> Of<T>() where T : OnlineMessage => Received.OfType<T>().ToList();

        public bool Any<T>(Func<T, bool> predicate) where T : OnlineMessage => Of<T>().Any(predicate);
    }

    private static Guest CreateGuest() => new();

    private OnlineHostSession CreateSut() => new(
        _relayRoomClient,
        _publisherProvider,
        _playerService,
        _diceGenerator,
        new CommandRegistry());

    private void SetupSuccessfulRelayCalls(FakeTransportPublisher publisher)
    {
        _relayRoomClient.Create(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(RoomSessionResult.Succeeded(
                "ABC123", "session-token", "Host", Guid.NewGuid(), Guid.NewGuid())));
        _relayRoomClient.GetRelayTicket("ABC123", "session-token", Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(RelayTicketResult.Succeeded(
                "relay-ticket", DateTimeOffset.UtcNow.AddMinutes(10))));
        _relayRoomClient.Ready("ABC123", "session-token", Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(RoomOperationResult.Succeeded()));
        _publisherProvider.Create("ABC123", "relay-ticket", Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<ITransportPublisher>(_room.Join(publisher)));
    }

    private void SetupDeterministicDice(params int[] values)
    {
        var queue = new Queue<int>(values);
        _diceGenerator.GetNextDiceResult(Arg.Any<int[]>())
            .Returns(_ => queue.Count == 0 ? values[^1] : queue.Dequeue());
    }

    private static Task Settle() => Task.Delay(SettleDelay);

    private static async Task WaitFor(Func<bool> condition)
    {
        var limit = DateTime.UtcNow + WaitTimeout;
        while (!condition() && DateTime.UtcNow < limit)
        {
            await Task.Delay(10);
        }

        condition().ShouldBeTrue("condition was not met within the timeout");
    }

    private async Task<OnlineHostSession> HostAsync(FakeTransportPublisher publisher, Rules rule = Rules.krExtended)
    {
        SetupSuccessfulRelayCalls(publisher);
        var sut = CreateSut();
        var result = await sut.HostAsync(rule);
        result.Success.ShouldBeTrue(result.Error ?? "hosting failed");
        await Settle();
        return sut;
    }

    [Fact]
    public async Task HostRunsRelayCallsInOrderAndReturnsRoomCode()
    {
        var publisher = new FakeTransportPublisher();
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);

        sut.RoomCode.ShouldBe("ABC123");
        sut.Game.ShouldNotBeNull();
        sut.HostPlayer.ShouldNotBeNull();
        sut.Game!.Players.ShouldHaveSingleItem();

        Received.InOrder(() =>
        {
            _relayRoomClient.Create(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            _relayRoomClient.GetRelayTicket("ABC123", "session-token", Arg.Any<CancellationToken>());
            _publisherProvider.Create("ABC123", "relay-ticket", Arg.Any<CancellationToken>());
            _relayRoomClient.Ready("ABC123", "session-token", Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task HostFailsWhenCreateIsRejected()
    {
        _relayRoomClient.Create(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(RoomSessionResult.Failed(
                new RelayClientError(RelayClientErrorCode.Unknown, "hub down"))));
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = CreateSut();

        var result = await sut.HostAsync(Rules.krExtended);

        result.Success.ShouldBeFalse();
        result.Error.ShouldBe("hub down");
        await _publisherProvider.DidNotReceive().Create(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _relayRoomClient.DidNotReceive().GetRelayTicket(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        sut.Game.ShouldBeNull();
    }

    [Fact]
    public async Task HostFailsWhenReadyIsRejectedAndDisposesPublisher()
    {
        var publisher = new FakeTransportPublisher();
        SetupSuccessfulRelayCalls(publisher);
        _relayRoomClient.Ready("ABC123", "session-token", Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(RoomOperationResult.Failed(
                new RelayClientError(RelayClientErrorCode.Unknown, "not ready"))));
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = CreateSut();

        var result = await sut.HostAsync(Rules.krExtended);

        result.Success.ShouldBeFalse();
        result.RoomCode.ShouldBe("ABC123");
        result.Error.ShouldBe("not ready");
        publisher.DisposeCount.ShouldBeGreaterThanOrEqualTo(1);
        sut.Game.ShouldBeNull();
    }

    [Fact]
    public async Task HostFailsWhenNoCurrentPlayer()
    {
        _playerService.CurrentPlayer.Returns((IPlayer)null!);
        await using var sut = CreateSut();

        var result = await sut.HostAsync(Rules.krExtended);

        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
        sut.Game.ShouldBeNull();
        sut.HostPlayer.ShouldBeNull();
    }

    [Fact]
    public async Task HostFailsWhenPublisherCreationThrows()
    {
        SetupSuccessfulRelayCalls(new FakeTransportPublisher());
        _publisherProvider.Create(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ITransportPublisher>(
                new InvalidOperationException("connection refused")));
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = CreateSut();

        var result = await sut.HostAsync(Rules.krExtended);

        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("connection refused");
        await _relayRoomClient.DidNotReceive().Ready(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        sut.Game.ShouldBeNull();
    }

    [Fact]
    public async Task GuestJoinIsBroadcastWithEchoedJoinRequestIdPrecedingSnapshot()
    {
        var publisher = new FakeTransportPublisher();
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);
        var guest = CreateGuest();
        guest.Attach(_room);

        await guest.Send(new JoinGameCommand { PlayerId = "join-1", Name = "Guest" });
        await WaitFor(() => guest.Any<GameStateBroadcast>(_ => true));

        var joined = guest.Of<PlayerJoinedBroadcast>().Single();
        joined.JoinRequestId.ShouldBe("join-1");
        joined.Type.ShouldBe(PlayerType.Network);
        joined.Name.ShouldBe("Guest");

        var snapshotIndex = guest.Received.IndexOf(guest.Of<GameStateBroadcast>().Single());
        guest.Received.IndexOf(joined).ShouldBeLessThan(snapshotIndex);

        var seatedGuest = sut.Game!.Players.Single(p => p.Type == PlayerType.Network);
        joined.PlayerId.ShouldBe(seatedGuest.InGameId);
        guest.Of<GameStateBroadcast>().Single().State.Players.Select(p => p.Name)
            .ShouldContain("Host");
        guest.Of<GameStateBroadcast>().Single().State.Players.Select(p => p.Name)
            .ShouldContain("Guest");
    }

    [Fact]
    public async Task JoinBeyondFourPlayersIsIgnored()
    {
        var publisher = new FakeTransportPublisher();
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);

        var guests = new List<Guest>();
        for (var i = 0; i < 3; i++)
        {
            var guest = CreateGuest();
            guest.Attach(_room);
            guests.Add(guest);
            await guest.Send(new JoinGameCommand { PlayerId = $"join-{i}", Name = $"Guest{i}" });
        }

        await WaitFor(() => sut.Game!.Players.Count == 4);

        var fifth = CreateGuest();
        fifth.Attach(_room);
        await fifth.Send(new JoinGameCommand { PlayerId = "join-5", Name = "Fifth" });
        await Settle();

        sut.Game!.Players.Count.ShouldBe(4);
        fifth.Of<PlayerJoinedBroadcast>().ShouldBeEmpty();
    }

    [Fact]
    public async Task ReadinessBroadcastsAndTurnChangedToFirstReadyPlayer()
    {
        var publisher = new FakeTransportPublisher();
        SetupDeterministicDice(1, 2, 3, 4, 5);
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);
        var guest = CreateGuest();
        guest.Attach(_room);
        await guest.Send(new JoinGameCommand { PlayerId = "join-1", Name = "Guest" });
        var guestId = await WaitForGuestIdAsync(guest);

        sut.SubmitLocalCommand(new ReadyCommand { PlayerId = sut.HostPlayer!.InGameId, IsReady = true });
        await guest.Send(new ReadyCommand { PlayerId = guestId, IsReady = true });
        await Settle();

        sut.Game!.IsPlaying.ShouldBeTrue();
        sut.Game.CurrentPlayer!.InGameId.ShouldBe(sut.HostPlayer.InGameId);

        var readyForHost = guest.Of<PlayerReadyBroadcast>()
            .SingleOrDefault(b => b.PlayerId == sut.HostPlayer.InGameId);
        readyForHost.ShouldNotBeNull();
        readyForHost!.IsReady.ShouldBeTrue();
        var readyForGuest = guest.Of<PlayerReadyBroadcast>()
            .SingleOrDefault(b => b.PlayerId == guestId);
        readyForGuest.ShouldNotBeNull();
        readyForGuest!.IsReady.ShouldBeTrue();
        guest.Of<TurnChangedBroadcast>().ShouldContain(b => b.PlayerId == sut.HostPlayer.InGameId);
    }

    [Fact]
    public async Task RollFixAndApplyScoreProduceCorrectBroadcasts()
    {
        var publisher = new FakeTransportPublisher();
        SetupDeterministicDice(1, 1, 1, 4, 5, 2, 2, 2, 3, 3);
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);
        var listener = CreateGuest();
        listener.Attach(_room);
        var hostId = sut.HostPlayer!.InGameId;

        sut.SubmitLocalCommand(new ReadyCommand { PlayerId = hostId, IsReady = true });
        await WaitFor(() => listener.Any<TurnChangedBroadcast>(b => b.PlayerId == hostId));

        sut.SubmitLocalCommand(new RollCommand { PlayerId = hostId });
        await WaitFor(() =>
            listener.Any<DiceRolledBroadcast>(b => b.Values.SequenceEqual([1, 1, 1, 4, 5])));

        sut.SubmitLocalCommand(new FixDiceCommand { PlayerId = hostId, Value = 1, IsFixed = true });
        await WaitFor(() => listener.Any<DiceFixedBroadcast>(b =>
            b.PlayerId == hostId && b.Value == 1 && b.IsFixed && !b.All));

        var result = sut.Game!.CurrentPlayer!.GetResultForScore(Scores.Ones);
        result.ShouldNotBeNull();
        result!.PossibleValue.ShouldBe(3);
        sut.SubmitLocalCommand(new ApplyScoreCommand { PlayerId = hostId, ScoreType = Scores.Ones });
        await WaitFor(() => listener.Any<ScoreAppliedBroadcast>(b =>
            b.PlayerId == hostId && b.ScoreType == Scores.Ones && b.Value == 3 && !b.HasBonus));

        listener.Of<TurnChangedBroadcast>().Count.ShouldBeGreaterThanOrEqualTo(2);
        sut.Game.CurrentPlayer!.InGameId.ShouldBe(hostId);
        sut.Game.CurrentPlayer!.IsScoreFilled(Scores.Ones).ShouldBeTrue();
        sut.Game.CurrentPlayer!.Roll.ShouldBe(1);
    }

    [Fact]
    public async Task OutOfTurnCommandIsIgnoredWithoutBroadcast()
    {
        var publisher = new FakeTransportPublisher();
        SetupDeterministicDice(1, 2, 3, 4, 5);
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);
        var guest = CreateGuest();
        guest.Attach(_room);
        await guest.Send(new JoinGameCommand { PlayerId = "join-1", Name = "Guest" });
        var guestId = await WaitForGuestIdAsync(guest);

        await guest.Send(new RollCommand { PlayerId = guestId });
        await Settle();

        guest.Of<DiceRolledBroadcast>().ShouldBeEmpty();
        sut.Game!.IsPlaying.ShouldBeFalse();
    }

    [Fact]
    public async Task CommandFromUnseatedPlayerIdIsIgnored()
    {
        var publisher = new FakeTransportPublisher();
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);
        var guest = CreateGuest();
        guest.Attach(_room);

        await guest.Send(new ReadyCommand { PlayerId = "unknown-id", IsReady = true });
        await Settle();

        guest.Of<PlayerReadyBroadcast>().ShouldBeEmpty();
        sut.Game!.Players.Select(p => p.IsReady).ShouldNotContain(true);
    }

    [Fact]
    public async Task GuestCommandUsingHostPlayerIdIsIgnored()
    {
        var publisher = new FakeTransportPublisher();
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);
        var guest = CreateGuest();
        guest.Attach(_room);

        await guest.Send(new ReadyCommand { PlayerId = sut.HostPlayer!.InGameId, IsReady = true });
        await Settle();

        guest.Of<PlayerReadyBroadcast>().ShouldBeEmpty();
        sut.HostPlayer!.IsReady.ShouldBeFalse();
    }

    [Fact]
    public async Task ApplyScoreBeforeRollIsRejected()
    {
        var publisher = new FakeTransportPublisher();
        SetupDeterministicDice(1, 1, 1, 4, 5);
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);
        var listener = CreateGuest();
        listener.Attach(_room);
        var hostId = sut.HostPlayer!.InGameId;

        sut.SubmitLocalCommand(new ReadyCommand { PlayerId = hostId, IsReady = true });
        await WaitFor(() => listener.Any<TurnChangedBroadcast>(b => b.PlayerId == hostId));

        sut.SubmitLocalCommand(new ApplyScoreCommand { PlayerId = hostId, ScoreType = Scores.Ones });
        await Settle();

        listener.Of<ScoreAppliedBroadcast>().ShouldBeEmpty();
        sut.Game!.CurrentPlayer!.IsScoreFilled(Scores.Ones).ShouldBeFalse();
    }

    [Fact]
    public async Task CreateBeforeFirstRollPublishesCompleteSnapshotOnRequest()
    {
        var publisher = new FakeTransportPublisher();
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);
        var guest = CreateGuest();
        guest.Attach(_room);
        await guest.Send(new JoinGameCommand { PlayerId = "join-1", Name = "Guest" });
        var guestId = await WaitForGuestIdAsync(guest);

        await guest.Send(new RequestGameStateCommand { PlayerId = guestId });
        await WaitFor(() => guest.Any<GameStateBroadcast>(_ => true));

        var state = guest.Of<GameStateBroadcast>().Last().State;
        state.Players.Count.ShouldBe(2);
        state.Players.Single(p => p.Name == "Host").Scores
            .Count(s => s.ScoreType == Scores.Ones && !s.HasValue && s.Value == 0)
            .ShouldBe(1);
        state.FixedDiceValues.ShouldBeEmpty();
        state.LastDiceValues.ShouldBeEmpty();
    }

    [Fact]
    public async Task LeavingGuestIsBroadcast()
    {
        var publisher = new FakeTransportPublisher();
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);
        var guest = CreateGuest();
        guest.Attach(_room);
        await guest.Send(new JoinGameCommand { PlayerId = "join-1", Name = "Guest" });
        var guestId = await WaitForGuestIdAsync(guest);

        await guest.Send(new LeaveGameCommand { PlayerId = guestId });
        await WaitFor(() => guest.Any<PlayerLeftBroadcast>(b => b.PlayerId == guestId));

        sut.Game!.Players.Count.ShouldBe(1);
    }

    [Fact]
    public async Task DisposePublishesGameEndedHostLeftToGuests()
    {
        var publisher = new FakeTransportPublisher();
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        var sut = await HostAsync(publisher);
        var guest = CreateGuest();
        guest.Attach(_room);

        await sut.DisposeAsync();
        await WaitFor(() => guest.Any<GameEndedBroadcast>(b => b.Reason == GameEndReason.HostLeft));

        sut.Game.ShouldBeNull();
        publisher.DisposeCount.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GameStateSnapshotPreservesDuplicateFixedDiceAndUnfilledZero()
    {
        var publisher = new FakeTransportPublisher();
        SetupDeterministicDice(5, 5, 6, 1, 2);
        _playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        await using var sut = await HostAsync(publisher);
        var listener = CreateGuest();
        listener.Attach(_room);
        var hostId = sut.HostPlayer!.InGameId;

        sut.SubmitLocalCommand(new ReadyCommand { PlayerId = hostId, IsReady = true });
        await WaitFor(() => listener.Any<TurnChangedBroadcast>(b => b.PlayerId == hostId));
        sut.SubmitLocalCommand(new RollCommand { PlayerId = hostId });
        await WaitFor(() => listener.Any<DiceRolledBroadcast>(_ => true));

        sut.Game!.FixDice(5, true);
        sut.Game.FixDice(5, true);
        await Settle();

        var state = GameStateMapper.Map(sut.Game!);
        state.FixedDiceValues.ShouldBe(new[] { 5, 5 });
        state.Players.ShouldHaveSingleItem();
        state.Players.Single().Scores
            .Count(s => s.ScoreType == Scores.Ones && !s.HasValue && s.Value == 0)
            .ShouldBe(1);
    }

    private static async Task<string> WaitForGuestIdAsync(Guest guest)
    {
        await WaitFor(() => guest.Any<PlayerJoinedBroadcast>(_ => true));
        return guest.Of<PlayerJoinedBroadcast>().Single().PlayerId;
    }
}
