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
using Xunit;

namespace MagicalYatzy.Core.Tests.Online;

public class OnlineClientSessionTests
{
    private const string RoomCode = "ABC123";

    private static readonly TimeSpan SettleDelay = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

    private readonly FakeRelayRoom _room = new();
    private readonly IRelayRoomClient _relayRoomClient = Substitute.For<IRelayRoomClient>();
    private readonly IRelayPublisherProvider _publisherProvider = Substitute.For<IRelayPublisherProvider>();
    private readonly IPlayerService _hostPlayerService = Substitute.For<IPlayerService>();
    private readonly IDiceGenerator _diceGenerator = Substitute.For<IDiceGenerator>();
    private readonly List<FakeTransportPublisher> _publishers = [];

    private OnlineHostSession CreateHostSession() => new(
        _relayRoomClient,
        _publisherProvider,
        _hostPlayerService,
        _diceGenerator,
        new CommandRegistry());

    private OnlineClientSession CreateGuestSession(string playerName)
    {
        var playerService = Substitute.For<IPlayerService>();
        playerService.CurrentPlayer.Returns(new Player(PlayerType.Local, playerName));
        return new OnlineClientSession(
            _relayRoomClient,
            _publisherProvider,
            playerService,
            new CommandRegistry());
    }

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
                _publishers.Add(publisher);
                return Task.FromResult<ITransportPublisher>(_room.Join(publisher));
            });
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

    private async Task<OnlineHostSession> HostAsync(Rules rule = Rules.krExtended)
    {
        SetupRelayCalls();
        _hostPlayerService.CurrentPlayer.Returns(new Player(PlayerType.Local, "Host"));
        var host = CreateHostSession();
        var result = await host.HostAsync(rule);
        result.Success.ShouldBeTrue(result.Error ?? "hosting failed");
        host.Game!.RoundTimeout = TimeSpan.Zero;
        await Settle();
        return host;
    }

    private async Task<OnlineClientSession> JoinGuestAsync(string playerName)
    {
        var guest = CreateGuestSession(playerName);
        var result = await guest.JoinAsync(RoomCode);
        result.Success.ShouldBeTrue(result.Error ?? $"{playerName} join failed");
        return guest;
    }

    private sealed class GameEventRecorder
    {
        public int TurnChanged;
        public int DiceRolled;
        public int ResultApplied;
        public int GameFinished;

        public static GameEventRecorder Attach(ClientYatzyGame game)
        {
            var recorder = new GameEventRecorder();
            game.TurnChanged += (_, _) => Interlocked.Increment(ref recorder.TurnChanged);
            game.DiceRolled += (_, _) => Interlocked.Increment(ref recorder.DiceRolled);
            game.ResultApplied += (_, _) => Interlocked.Increment(ref recorder.ResultApplied);
            game.GameFinished += (_, _) => Interlocked.Increment(ref recorder.GameFinished);
            return recorder;
        }
    }

    [Fact]
    public async Task TwoGuestsJoinHydrateAndVerifyLocalIdsAndProjections()
    {
        await using var host = await HostAsync();

        var guest1 = await JoinGuestAsync("Guest1");
        var guest2 = await JoinGuestAsync("Guest2");
        // the first guest learns about the second join through broadcasts
        await WaitFor(() => guest1.Game!.Players.Count == 3 && guest2.Game!.Players.Count == 3);

        var hostGame = host.Game!;
        hostGame.Players.Count.ShouldBe(3);

        foreach (var guest in new[] { guest1, guest2 })
        {
            guest.RoomCode.ShouldBe(RoomCode);
            guest.Game.ShouldNotBeNull();
            guest.LocalPlayer.ShouldNotBeNull();
            guest.LocalPlayer!.Type.ShouldBe(PlayerType.Local);
            guest.Game!.Players.Count.ShouldBe(3);
            guest.Game.Players.Select(p => p.Name)
                .ShouldBe(["Host", "Guest1", "Guest2"]);
        }

        // each guest sees its own seat as local and every other seat as network
        guest1.Game!.Players.Select(p => p.Type)
            .ShouldBe([PlayerType.Network, PlayerType.Local, PlayerType.Network]);
        guest2.Game!.Players.Select(p => p.Type)
            .ShouldBe([PlayerType.Network, PlayerType.Network, PlayerType.Local]);

        // server-assigned ids match the host seats
        var guest1Id = guest1.LocalPlayer!.InGameId!;
        var guest2Id = guest2.LocalPlayer!.InGameId!;
        hostGame.Players.Single(p => p.InGameId == guest1Id).Name.ShouldBe("Guest1");
        hostGame.Players.Single(p => p.InGameId == guest2Id).Name.ShouldBe("Guest2");
    }

    [Fact]
    public async Task FullGameWithHostAndTwoGuestsPlaysToCompletionWithMatchingProjections()
    {
        SetupDeterministicDice(1, 2, 3, 4, 5);
        await using var host = await HostAsync();
        var hostGame = host.Game!;
        var hostId = host.HostPlayer!.InGameId!;

        var guest1 = await JoinGuestAsync("Guest1");
        var guest2 = await JoinGuestAsync("Guest2");
        var guest1Events = GameEventRecorder.Attach(guest1.Game!);
        var guest2Events = GameEventRecorder.Attach(guest2.Game!);

        // everyone signals ready through their own session
        host.SubmitLocalCommand(new ReadyCommand { PlayerId = hostId, IsReady = true });
        guest1.Game!.SetPlayerReady(guest1.LocalPlayer!, true);
        guest2.Game!.SetPlayerReady(guest2.LocalPlayer!, true);
        await WaitFor(() => hostGame.IsPlaying);

        // every turn: one roll, then the first open score - until the game ends
        var turns = 0;
        while (hostGame.IsPlaying && turns++ < 500)
        {
            var currentPlayerId = hostGame.CurrentPlayer!.InGameId!;
            var guest = currentPlayerId == hostId
                ? null
                : new[] { guest1, guest2 }.Single(g => g.LocalPlayer!.InGameId == currentPlayerId);

            // the acting session sends the roll intent through its own projection, so wait
            // for the projection to observe the turn before rolling
            if (guest != null)
            {
                await WaitFor(() => guest.Game!.CurrentPlayer != null
                    && guest.Game.CurrentPlayer.InGameId == currentPlayerId);
                guest.Game!.ReportRoll();
            }
            else
            {
                host.SubmitLocalCommand(new RollCommand { PlayerId = hostId });
            }

            await WaitFor(() => hostGame.CurrentPlayer != null && hostGame.CurrentPlayer.Roll >= 2);

            var openScore = hostGame.CurrentPlayer!.Results!
                .First(r => r.ScoreType != Scores.Bonus && !r.HasValue).ScoreType;
            if (guest == null)
            {
                host.SubmitLocalCommand(new ApplyScoreCommand { PlayerId = hostId, ScoreType = openScore });
            }
            else
            {
                guest.Game!.ApplyScore(new RollResult(openScore, hostGame.Rules.CurrentRule));
            }

            await WaitFor(() => !hostGame.IsPlaying
                || (hostGame.CurrentPlayer != null && hostGame.CurrentPlayer.InGameId != currentPlayerId));
            // let both projections catch up with the turn change before the next intent
            await WaitFor(() => !guest1.Game!.IsPlaying || guest1.Game.CurrentPlayer?.InGameId != currentPlayerId);
            await WaitFor(() => !guest2.Game!.IsPlaying || guest2.Game.CurrentPlayer?.InGameId != currentPlayerId);
        }

        await WaitFor(() => !guest1.Game!.IsPlaying && !guest2.Game!.IsPlaying);

        foreach (var (guest, events) in new[] { (guest1, guest1Events), (guest2, guest2Events) })
        {
            var game = guest.Game!;
            game.Round.ShouldBe(hostGame.Round);
            game.Round.ShouldBe(13);
            game.IsPlaying.ShouldBeFalse();
            // players ordered by the host standings
            game.Players.Select(p => p.InGameId!)
                .ShouldBe(hostGame.Players.Select(p => p.InGameId));
            game.CurrentPlayer!.InGameId.ShouldBe(hostGame.CurrentPlayer!.InGameId);

            foreach (var hostPlayer in hostGame.Players)
            {
                var projected = game.Players.Single(p => p.InGameId == hostPlayer.InGameId);
                projected.Total.ShouldBe(hostPlayer.Total);
                foreach (var hostResult in hostPlayer.Results!)
                {
                    var projectedResult = projected.GetResultForScore(hostResult.ScoreType);
                    projectedResult.ShouldNotBeNull();
                    projectedResult!.Value.ShouldBe(hostResult.Value);
                    projectedResult.HasValue.ShouldBe(hostResult.HasValue);
                    projectedResult.HasBonus.ShouldBe(hostResult.HasBonus);
                }
            }

            events.TurnChanged.ShouldBeGreaterThanOrEqualTo(39);
            events.DiceRolled.ShouldBeGreaterThanOrEqualTo(39);
            events.ResultApplied.ShouldBeGreaterThanOrEqualTo(39);
            events.GameFinished.ShouldBe(1);
        }
    }

    [Fact]
    public async Task HostDisposalProducesSingleGameEndedHostLeftOnGuests()
    {
        await using var host = await HostAsync();
        var guest = await JoinGuestAsync("Guest1");

        var reasons = new List<GameEndReason>();
        var finished = 0;
        guest.GameEnded += reasons.Add;
        guest.Game!.GameFinished += (_, _) => finished++;

        await host.DisposeAsync();

        await WaitFor(() => reasons.Count == 1);
        reasons.ShouldBe([GameEndReason.HostLeft]);
        // a game that never started still ends locally exactly once
        finished.ShouldBe(1);
        guest.Game.IsPlaying.ShouldBeFalse();

        await Settle();
        reasons.ShouldBe([GameEndReason.HostLeft]);
        finished.ShouldBe(1);
    }

    [Fact]
    public async Task GuestTransportDisconnectRaisesGameEndedHostDisconnectedAndSynthesizesGameFinished()
    {
        await using var host = await HostAsync();
        var guest = await JoinGuestAsync("Guest1");
        await WaitFor(() => _publishers.Count == 2);

        var reasons = new List<GameEndReason>();
        var finished = 0;
        guest.GameEnded += reasons.Add;
        guest.Game!.GameFinished += (_, _) => finished++;

        // simulate the transport losing the connection to the host
        _publishers[^1].SetConnectionState(TransportConnectionState.Disconnected);

        await WaitFor(() => reasons.Count == 1);
        reasons.ShouldBe([GameEndReason.HostDisconnected]);
        finished.ShouldBe(1);
        guest.Game.IsPlaying.ShouldBeFalse();
    }

    [Fact]
    public async Task BothEndSignalsProduceOnlyOneGameEnd()
    {
        await using var host = await HostAsync();
        var guest = await JoinGuestAsync("Guest1");
        await WaitFor(() => _publishers.Count == 2);

        var reasons = new List<GameEndReason>();
        var finished = 0;
        guest.GameEnded += reasons.Add;
        guest.Game!.GameFinished += (_, _) => finished++;

        await host.DisposeAsync();
        await WaitFor(() => reasons.Count == 1);
        _publishers[^1].SetConnectionState(TransportConnectionState.Disconnected);
        await Settle();

        reasons.ShouldBe([GameEndReason.HostLeft]);
        finished.ShouldBe(1);
    }

    [Fact]
    public async Task JoinFailsWhenRelayRejectsTheRoom()
    {
        SetupRelayCalls();
        _relayRoomClient.Join(RoomCode, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(RoomSessionResult.Failed(
                new RelayClientError(RelayClientErrorCode.Unknown, "room not found"))));

        var guest = CreateGuestSession("Guest1");
        var result = await guest.JoinAsync(RoomCode);

        result.Success.ShouldBeFalse();
        result.Error.ShouldBe("room not found");
        guest.Game.ShouldBeNull();
        await _publisherProvider.DidNotReceive().Create(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinTimesOutWhenHostNeverSendsSnapshot()
    {
        SetupRelayCalls();

        var guest = CreateGuestSession("Guest1");
        guest.JoinTimeout = TimeSpan.FromMilliseconds(300);
        var result = await guest.JoinAsync(RoomCode);

        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("Timed out");
        result.RoomCode.ShouldBe(RoomCode);
        guest.Game.ShouldBeNull();
        // relay resources created for the failed join are disposed
        _publishers.ShouldHaveSingleItem();
        _publishers[0].DisposeCount.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task JoinTimesOutWhenHostIsFull()
    {
        await using var host = await HostAsync();
        await JoinGuestAsync("Guest1");
        await JoinGuestAsync("Guest2");
        await JoinGuestAsync("Guest3");
        await WaitFor(() => host.Game!.Players.Count == 4);

        var fifth = CreateGuestSession("Fifth");
        fifth.JoinTimeout = TimeSpan.FromMilliseconds(300);
        var result = await fifth.JoinAsync(RoomCode);

        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("Timed out");
        fifth.Game.ShouldBeNull();
        host.Game!.Players.Count.ShouldBe(4);
    }

    [Fact]
    public async Task JoinFailsWhenNoCurrentPlayerIsLoggedIn()
    {
        SetupRelayCalls();
        var playerService = Substitute.For<IPlayerService>();
        playerService.CurrentPlayer.Returns((IPlayer)null!);
        var guest = new OnlineClientSession(
            _relayRoomClient,
            _publisherProvider,
            playerService,
            new CommandRegistry());

        var result = await guest.JoinAsync(RoomCode);

        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
        guest.Game.ShouldBeNull();
        await _relayRoomClient.DidNotReceive().Join(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinFailsWhenPublisherCreationThrows()
    {
        SetupRelayCalls();
        _publisherProvider.Create(RoomCode, "relay-ticket", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ITransportPublisher>(
                new InvalidOperationException("connection refused")));

        var guest = CreateGuestSession("Guest1");
        var result = await guest.JoinAsync(RoomCode);

        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("connection refused");
        guest.Game.ShouldBeNull();
    }

    [Fact]
    public async Task DisposedGuestSessionFailsJoin()
    {
        SetupRelayCalls();
        var guest = CreateGuestSession("Guest1");
        await guest.DisposeAsync();

        var result = await guest.JoinAsync(RoomCode);

        result.Success.ShouldBeFalse();
        result.Error?.ShouldContain("disposed");
    }
}
