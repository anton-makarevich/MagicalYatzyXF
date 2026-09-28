using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MagicalYatzy.Core.Tests.Online.Harness;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.Transport;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.Online;

/// <summary>
/// Whole-game coverage: a host and its guests play through the relay to the last round, restart
/// and play again, and see a guest or the host drop out mid-game.
/// </summary>
public class OnlineGameEndToEndTests
{
    private const int RoundsPerGame = 13;
    private static readonly TimeSpan AutoFillTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan EndSignalTimeout = TimeSpan.FromSeconds(5);

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task OnlineGamePlaysToTheEndAndRestartsForEveryone(int playerCount)
    {
        await using var harness = await OnlineGameHarness.StartAsync(playerCount);
        var hostGame = harness.HostGame;

        // every seat is taken and every guest knows which seat is its own
        hostGame.Players.Select(p => p.Name).ShouldBe(
            new[] { "Host" }.Concat(Enumerable.Range(1, playerCount - 1).Select(i => $"Guest{i}")));
        foreach (var guest in harness.Guests)
        {
            guest.Game!.Players.Count.ShouldBe(playerCount);
            guest.Game.Players.Single(p => p.InGameId == guest.LocalPlayer!.InGameId).Type
                .ShouldBe(PlayerType.Local);
            guest.Game.Players.Count(p => p.Type == PlayerType.Local).ShouldBe(1);
        }

        // the round timer is armed for the whole game, so the second turn can be auto-filled
        hostGame.RoundTimeout = AutoFillTimeout;
        harness.ReadyAll();
        await harness.WaitForPlayingAsync();

        // turn 1 is driven by hand: roll, keep one die, score the first open entry
        var firstActor = harness.CurrentPlayerId;
        var dice = await harness.RollAsync();
        dice.Count.ShouldBe(5);
        hostGame.FixedRollResults.ShouldBeEmpty();
        foreach (var guest in harness.Guests)
        {
            guest.Game!.LastDiceResult.DiceResults.ShouldBe(dice);
            guest.Game.FixedRollResults.ShouldBeEmpty();
        }

        var fixedValue = dice[0];
        await harness.FixDiceAsync(fixedValue);
        hostGame.FixedRollResults.ShouldBe([fixedValue]);
        hostGame.NumberOfFixedDice.ShouldBe(1);
        foreach (var guest in harness.Guests)
        {
            guest.Game!.FixedRollResults.ShouldBe([fixedValue]);
            guest.Game.NumberOfFixedDice.ShouldBe(1);
        }

        var firstScore = OnlineGameHarness.FirstOpenScore(hostGame.CurrentPlayer!);
        await harness.ApplyScoreAsync(firstScore);
        await harness.WaitForProjectionsAgreeAsync();
        AssertCommittedEverywhere(harness, firstActor, firstScore);

        // the turn that follows is already timed, later turns are played by hand. Changing the
        // timeout now leaves the armed timer of the current turn alone, so only it can expire.
        var timedActor = harness.CurrentPlayerId;
        var timedScore = OnlineGameHarness.FirstOpenScore(hostGame.CurrentPlayer!);
        hostGame.RoundTimeout = TimeSpan.Zero;
        var rollsBeforeTimeout = harness.GameEvents(harness.Guests[0]).DiceRolled;
        await harness.WaitForAppliedAsync(timedActor, timedScore);
        hostGame.Players.Single(p => p.InGameId == timedActor).GetResultForScore(timedScore)!
            .Value.ShouldBe(0, "the timed out turn has no roll, so its entry is filled with zero");
        AssertCommittedEverywhere(harness, timedActor, timedScore);
        // no dice were rolled for the turn the timer filled
        harness.GameEvents(harness.Guests[0]).DiceRolled.ShouldBe(rollsBeforeTimeout);

        await harness.PlayToCompletionAsync();

        hostGame.Round.ShouldBe(RoundsPerGame);
        hostGame.IsPlaying.ShouldBeFalse();
        hostGame.Players.Select(p => p.Total)
            .ShouldBe(hostGame.Players.Select(p => p.Total).OrderByDescending(total => total));
        foreach (var guest in harness.Guests)
        {
            guest.Game!.IsPlaying.ShouldBeFalse();
            guest.Game.Players.ShouldAllBe(p => !p.IsReady);
            // every seat filled one entry per round
            guest.Game.Players.ShouldAllBe(p => p.Results!
                .Count(r => r.HasValue && r.ScoreType != Scores.Bonus) == RoundsPerGame);
            var events = harness.GameEvents(guest);
            events.GameFinished.ShouldBe(1);
            // one roll per turn, except the turn the round timer filled
            events.DiceRolled.ShouldBe(playerCount * RoundsPerGame - 1);
            events.ResultApplied.ShouldBeGreaterThanOrEqualTo(playerCount * RoundsPerGame);
        }

        harness.AssertProjectionsMatchHost();

        // the standings leader keeps the table, everyone else moves one seat back
        var leaderId = hostGame.Players[0].InGameId;
        await harness.RestartAsync();

        hostGame.Round.ShouldBe(1);
        hostGame.IsPlaying.ShouldBeFalse();
        hostGame.Players.Select(p => p.SeatNo).ShouldBe(Enumerable.Range(0, playerCount));
        hostGame.Players[^1].InGameId.ShouldBe(leaderId);
        hostGame.Players.ShouldAllBe(p => !p.IsReady);
        hostGame.Players.SelectMany(p => p.Results!).ShouldAllBe(r => !r.HasValue);
        foreach (var guest in harness.Guests)
        {
            var game = guest.Game!;
            // the guest still owns the very same seat, so it can act in the new game
            game.Players.ShouldContain(p => p.InGameId == guest.LocalPlayer!.InGameId);
            game.Players.Single(p => p.InGameId == guest.LocalPlayer!.InGameId).Type
                .ShouldBe(PlayerType.Local);
            // the finished game is not replayed into the new one
            harness.GameEvents(guest).GameFinished.ShouldBe(1);
        }

        // readying up again starts a second game that the guests play on their own seats
        harness.ReadyAll();
        await harness.WaitForPlayingAsync();
        await harness.WaitForProjectionsAgreeAsync();
        hostGame.Round.ShouldBe(1);
        var secondTurn = await harness.DriveTurnAsync();
        hostGame.Players.Single(p => p.InGameId == secondTurn.PlayerId)
            .GetResultForScore(secondTurn.ScoreType)!.HasValue.ShouldBeTrue();
        await harness.WaitForProjectionsAgreeAsync();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task GuestQuittingMidTurnIsSeatedOutAndTheTableFinishesTheGame(int playerCount)
    {
        await using var harness = await OnlineGameHarness.StartAsync(playerCount);
        var hostGame = harness.HostGame;
        harness.ReadyAll();
        await harness.WaitForPlayingAsync();

        // play until a guest owns the turn, then let that guest roll and abandon the turn
        while (harness.CurrentPlayerId == harness.HostPlayerId)
        {
            await harness.DriveTurnAsync();
        }

        var leaving = harness.GuestFor(harness.CurrentPlayerId)!;
        var leavingId = leaving.LocalPlayer!.InGameId!;
        var observers = harness.Guests.Where(g => g != leaving).ToList();
        var leftSeats = observers.ToDictionary(g => g, _ => new List<string>());
        foreach (var observer in observers)
        {
            observer.Game!.PlayerLeft += (_, args) => leftSeats[observer].Add(args.Player.InGameId!);
        }

        await harness.RollAsync();
        var publisher = harness.PublisherFor(leaving);
        await harness.LeaveAsync(leaving);

        // the leaving session tore its transport down and the host dropped its seat
        publisher.DisposeCount.ShouldBeGreaterThanOrEqualTo(1);
        hostGame.Players.Count.ShouldBe(playerCount - 1);
        hostGame.Players.ShouldAllBe(p => p.InGameId != leavingId);
        // the game is not over: the rest of the table carries on
        hostGame.IsPlaying.ShouldBeTrue();
        hostGame.CurrentPlayer!.InGameId.ShouldNotBe(leavingId);
        foreach (var observer in observers)
        {
            leftSeats[observer].ShouldBe([leavingId]);
            observer.Game!.Players.ShouldAllBe(p => p.InGameId != leavingId);
        }

        await harness.PlayToCompletionAsync();
        harness.AssertProjectionsMatchHost();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task HostLeavingEndsTheGameOnEveryGuestExactlyOnce(int playerCount)
    {
        await using var harness = await OnlineGameHarness.StartAsync(playerCount);
        harness.ReadyAll();
        await harness.WaitForPlayingAsync();
        await harness.DriveTurnAsync();

        await harness.Host.DisposeAsync();

        foreach (var guest in harness.Guests)
        {
            (await harness.SessionEvents(guest).WaitForFirstGameEndAsync(EndSignalTimeout))
                .ShouldBe(GameEndReason.HostLeft);
            harness.GameEvents(guest).GameFinished.ShouldBe(1);
            guest.Game!.IsPlaying.ShouldBeFalse();
        }

        // a later end signal must not end the game a second time
        foreach (var guest in harness.Guests)
        {
            harness.PublisherFor(guest).SetConnectionState(TransportConnectionState.Disconnected);
        }

        await harness.SettleAsync();
        foreach (var guest in harness.Guests)
        {
            harness.SessionEvents(guest).GameEndedReasons.ShouldBe([GameEndReason.HostLeft]);
            harness.GameEvents(guest).GameFinished.ShouldBe(1);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task LosingTheRelayEndsTheGameOnEveryGuestExactlyOnce(int playerCount)
    {
        await using var harness = await OnlineGameHarness.StartAsync(playerCount);
        harness.ReadyAll();
        await harness.WaitForPlayingAsync();
        await harness.DriveTurnAsync();

        // every client loses the relay connection while the host is still there
        foreach (var guest in harness.Guests)
        {
            harness.PublisherFor(guest).SetConnectionState(TransportConnectionState.Disconnected);
        }

        foreach (var guest in harness.Guests)
        {
            (await harness.SessionEvents(guest).WaitForFirstGameEndAsync(EndSignalTimeout))
                .ShouldBe(GameEndReason.HostDisconnected);
            harness.GameEvents(guest).GameFinished.ShouldBe(1);
            guest.Game!.IsPlaying.ShouldBeFalse();
        }

        // the host leaving afterwards must not end the game a second time
        await harness.Host.DisposeAsync();
        await harness.SettleAsync();
        foreach (var guest in harness.Guests)
        {
            harness.SessionEvents(guest).GameEndedReasons.ShouldBe([GameEndReason.HostDisconnected]);
            harness.GameEvents(guest).GameFinished.ShouldBe(1);
        }
    }

    /// <summary>
    /// Asserts the host committed a score and every guest projection carries the same value.
    /// </summary>
    private static void AssertCommittedEverywhere(OnlineGameHarness harness, string playerId, Scores scoreType)
    {
        var committed = harness.HostGame.Players
            .Single(p => p.InGameId == playerId)
            .GetResultForScore(scoreType)!;
        committed.HasValue.ShouldBeTrue();
        foreach (var guest in harness.Guests)
        {
            var projected = guest.Game!.Players
                .Single(p => p.InGameId == playerId)
                .GetResultForScore(scoreType)!;
            projected.HasValue.ShouldBeTrue();
            projected.Value.ShouldBe(committed.Value);
            projected.HasBonus.ShouldBe(committed.HasBonus);
        }
    }
}
