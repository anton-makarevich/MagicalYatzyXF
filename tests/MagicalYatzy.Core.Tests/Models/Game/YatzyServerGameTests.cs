using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Utils;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.Models.Game;

public class YatzyServerGameTests : IDisposable
{
    private YatzyServerGame _sut;

    public YatzyServerGameTests()
    {
        _sut = new YatzyServerGame();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    private void StartGame(IPlayer player)
    {
        _sut.JoinGame(player);
        _sut.SetPlayerReady(player, true);
    }

    [Fact]
    public void ServerGameCommitsAppliedResultValueToCurrentPlayerResults()
    {
        const Scores scoreToAdd = Scores.Ones;
        const Rules rule = Rules.krStandard;
        var player = Substitute.For<IPlayer>();
        player.IsReady.Returns(true);
        player.InGameId.Returns("0");
        var results = new List<RollResult>();
        foreach (var score in EnumUtils.GetValues<Scores>())
            results.Add(new RollResult(score, rule));
        player.Results.Returns(results);
        StartGame(player);

        _sut.ApplyScore(new RollResult(scoreToAdd, rule) { PossibleValue = 5 });

        var committedResult = results.First(f => f.ScoreType == scoreToAdd);
        committedResult.HasValue.ShouldBeTrue();
        committedResult.Value.ShouldBe(5);
    }

    [Fact]
    public void ServerGameCommitsNumericBonusAs35WhenTotalNumericAboveThreshold()
    {
        const Scores scoreToAdd = Scores.Ones;
        const Rules rule = Rules.krStandard;
        var player = Substitute.For<IPlayer>();
        player.IsReady.Returns(true);
        player.InGameId.Returns("0");
        player.TotalNumeric.Returns(65);
        var results = new List<RollResult>();
        foreach (var score in EnumUtils.GetValues<Scores>())
        {
            var result = new RollResult(score, rule);
            if (score != scoreToAdd && score != Scores.Bonus)
                result.Value = result.MaxValue;
            results.Add(result);
        }

        player.Results.Returns(results);
        StartGame(player);

        _sut.ApplyScore(new RollResult(scoreToAdd, rule) { PossibleValue = 5 });

        var bonusResult = results.First(f => f.ScoreType == Scores.Bonus);
        bonusResult.HasValue.ShouldBeTrue();
        bonusResult.Value.ShouldBe(35);
    }

    [Fact]
    public void ServerGameCommitsNumericBonusAsZeroWhenAllOtherNumericsFilledBelowThreshold()
    {
        const Scores scoreToAdd = Scores.Ones;
        const Rules rule = Rules.krStandard;
        var player = Substitute.For<IPlayer>();
        player.IsReady.Returns(true);
        player.InGameId.Returns("0");
        player.TotalNumeric.Returns(0);
        var results = new List<RollResult>();
        foreach (var score in EnumUtils.GetValues<Scores>())
        {
            var result = new RollResult(score, rule);
            if (score != scoreToAdd && score != Scores.Bonus)
                result.Value = 0;
            results.Add(result);
        }

        player.Results.Returns(results);
        StartGame(player);
        var appliedResults = new List<RollResultEventArgs>();
        _sut.ResultApplied += (_, args) => appliedResults.Add(args);

        _sut.ApplyScore(new RollResult(scoreToAdd, rule) { PossibleValue = 5 });

        appliedResults.Count.ShouldBe(2);
        appliedResults[1].ScoreType.ShouldBe(Scores.Bonus);
        appliedResults[1].Value.ShouldBe(0);
        var bonusResult = results.First(f => f.ScoreType == Scores.Bonus);
        bonusResult.HasValue.ShouldBeTrue();
        bonusResult.Value.ShouldBe(0);
    }

    [Fact]
    public void ServerGameCommitsKniffelBonusFlagWhenApplicable()
    {
        const Scores scoreToAdd = Scores.Ones;
        const Rules rule = Rules.krExtended;
        var diceGenerator = Substitute.For<IDiceGenerator>();
        diceGenerator.GetNextDiceResult().ReturnsForAnyArgs(1);
        _sut = new YatzyServerGame(rule, diceGenerator);
        var player = Substitute.For<IPlayer>();
        player.IsReady.Returns(true);
        player.InGameId.Returns("0");
        player.GetResultForScore(Scores.Kniffel).Returns(new RollResult(Scores.Kniffel, rule)
        {
            Value = 50
        });
        var results = new List<RollResult>();
        foreach (var score in EnumUtils.GetValues<Scores>())
            results.Add(new RollResult(score, rule));
        player.Results.Returns(results);
        StartGame(player);
        _sut.ReportRoll();
        var appliedResults = new List<RollResultEventArgs>();
        _sut.ResultApplied += (_, args) => appliedResults.Add(args);

        _sut.ApplyScore(new RollResult(scoreToAdd, rule) { PossibleValue = 5 });

        appliedResults.Count.ShouldBe(1);
        appliedResults[0].HasBonus.ShouldBeTrue();
        var committedResult = results.First(f => f.ScoreType == scoreToAdd);
        committedResult.HasValue.ShouldBeTrue();
        committedResult.Value.ShouldBe(5);
        committedResult.HasBonus.ShouldBeTrue();
    }

    [Fact]
    public void ServerGameRaisesTheSameResultAppliedEventsAsLocalGame()
    {
        var localEvents = ApplyScoreAndCaptureEvents(new YatzyGame());
        var serverEvents = ApplyScoreAndCaptureEvents(new YatzyServerGame());

        serverEvents.Count.ShouldBe(localEvents.Count);
        serverEvents.ShouldBe(localEvents);
    }

    private static List<(int Value, Scores ScoreType, bool HasBonus)> ApplyScoreAndCaptureEvents(YatzyGame game)
    {
        const Scores scoreToAdd = Scores.Ones;
        const Rules rule = Rules.krStandard;
        var player = Substitute.For<IPlayer>();
        player.IsReady.Returns(true);
        player.InGameId.Returns("0");
        player.TotalNumeric.Returns(65);
        var results = new List<RollResult>();
        foreach (var score in EnumUtils.GetValues<Scores>())
        {
            var result = new RollResult(score, rule);
            if (score != scoreToAdd && score != Scores.Bonus)
                result.Value = result.MaxValue;
            results.Add(result);
        }

        player.Results.Returns(results);
        game.JoinGame(player);
        game.SetPlayerReady(player, true);
        var appliedResults = new List<(int, Scores, bool)>();
        game.ResultApplied += (_, args) => appliedResults.Add((args.Value, args.ScoreType, args.HasBonus));

        game.ApplyScore(new RollResult(scoreToAdd, rule) { PossibleValue = 5 });

        return appliedResults;
    }

    [Fact]
    public void ServerGameRejectsScoreThatIsNotOnCurrentPlayerSheet()
    {
        const Rules rule = Rules.krSimple;
        _sut = new YatzyServerGame(rule, new RandomDiceGenerator()) { RoundTimeout = TimeSpan.Zero };
        var player = new Player(PlayerType.Local);
        StartGame(player);
        var appliedResults = new List<RollResultEventArgs>();
        _sut.ResultApplied += (_, args) => appliedResults.Add(args);
        var turnChangedCount = 0;
        _sut.TurnChanged += (_, _) => turnChangedCount++;
        //krSimple has no bonus score
        var detachedResult = new RollResult(Scores.Bonus, rule) { PossibleValue = 35 };

        _sut.ApplyScore(detachedResult);

        appliedResults.ShouldBeEmpty();
        turnChangedCount.ShouldBe(0);
        player.Results!.ShouldAllBe(f => !f.HasValue);
        detachedResult.HasValue.ShouldBeFalse();
        detachedResult.Value.ShouldBe(0);
    }

    [Fact]
    public void ServerGameRejectsScoreThatIsAlreadyFilledOnCurrentPlayerSheet()
    {
        const Rules rule = Rules.krSimple;
        _sut = new YatzyServerGame(rule, new RandomDiceGenerator()) { RoundTimeout = TimeSpan.Zero };
        var player = new Player(PlayerType.Local);
        StartGame(player);
        var appliedResults = new List<RollResultEventArgs>();
        _sut.ResultApplied += (_, args) => appliedResults.Add(args);
        var turnChangedCount = 0;
        _sut.TurnChanged += (_, _) => turnChangedCount++;
        var filledResult = player.Results!.First(f => f.ScoreType == Scores.Ones);
        filledResult.Value = 3;

        //same score type, but a fresh unfilled instance carrying a different value
        filledResult.PossibleValue = 5;
        _sut.ApplyScore(filledResult);

        appliedResults.ShouldBeEmpty();
        turnChangedCount.ShouldBe(0);
        filledResult.Value.ShouldBe(3);
    }

    [Fact]
    public async Task ServerGameRoundTimeoutWithoutRollCommitsZeroAfterRollInEarlierTurn()
    {
        const Rules rule = Rules.krSimple;
        var diceGenerator = Substitute.For<IDiceGenerator>();
        diceGenerator.GetNextDiceResult().ReturnsForAnyArgs(2);
        _sut = new YatzyServerGame(rule, diceGenerator) { RoundTimeout = TimeSpan.FromMilliseconds(200) };
        var player = new Player(PlayerType.Local);
        StartGame(player);
        //host refreshes values of the current roll, as a server host does
        _sut.ReportRoll();
        player.CheckRollResults(_sut.LastDiceResult, _sut.Rules);
        var twosResult = player.Results!.First(f => f.ScoreType == Scores.Twos);
        twosResult.PossibleValue.ShouldBe(10);
        var twosApplied = new TaskCompletionSource();
        _sut.ResultApplied += (_, e) =>
        {
            if (e.ScoreType == Scores.Twos)
                twosApplied.TrySetResult();
        };

        //ends the turn, the same player starts the next one without rolling
        _sut.ApplyScore(player.Results!.First(f => f.ScoreType == Scores.Ones));

        var completed = await Task.WhenAny(twosApplied.Task, Task.Delay(5000));
        completed.ShouldBe(twosApplied.Task);

        twosResult.HasValue.ShouldBeTrue();
        twosResult.Value.ShouldBe(0);
    }

    [Fact]
    public void NewGameAfterFinishedGameResetsResultsWithoutExplicitRestart()
    {
        const Rules rule = Rules.krBaby;
        _sut = new YatzyServerGame(rule, new RandomDiceGenerator()) { RoundTimeout = TimeSpan.Zero };
        var player = new Player(PlayerType.Local);
        StartGame(player);
        foreach (var result in player.Results!.ToList())
            _sut.ApplyScore(result);

        _sut.IsPlaying.ShouldBeFalse();
        player.Results!.ShouldAllBe(f => f.HasValue);

        //new game is started by readiness alone
        _sut.SetPlayerReady(player, true);

        _sut.IsPlaying.ShouldBeTrue();
        _sut.Round.ShouldBe(1);
        player.Results.ShouldAllBe(f => !f.HasValue);
    }

    [Fact]
    public void ServerGameRestartDoesNotAutoReadyPlayersUntilAllReportReadiness()
    {
        var player1 = new Player(PlayerType.Local);
        var player2 = new Player(PlayerType.Local);
        StartGame(player1);
        StartGame(player2);
        var turnChangedCount = 0;
        _sut.TurnChanged += (_, _) => turnChangedCount++;

        _sut.SetPlayerReady(player1, false);
        _sut.SetPlayerReady(player2, false);
        turnChangedCount = 0;

        _sut.RestartGame();

        _sut.IsPlaying.ShouldBeFalse();
        player1.IsReady.ShouldBeFalse();
        player2.IsReady.ShouldBeFalse();
        turnChangedCount.ShouldBe(0);

        _sut.SetPlayerReady(player1, true);
        _sut.IsPlaying.ShouldBeFalse();
        turnChangedCount.ShouldBe(0);

        _sut.SetPlayerReady(player2, true);
        _sut.IsPlaying.ShouldBeTrue();
        turnChangedCount.ShouldBe(1);
    }

    [Fact]
    public async Task ServerGameRoundTimeoutAutoFillsFirstOpenScoreAndAdvancesTurn()
    {
        _sut = new YatzyServerGame { RoundTimeout = TimeSpan.FromMilliseconds(200) };
        var player1 = new Player(PlayerType.Local);
        var player2 = new Player(PlayerType.Local);
        StartGame(player1);
        StartGame(player2);
        var turnAdvancedToNextPlayer = new TaskCompletionSource();
        _sut.TurnChanged += (_, e) =>
        {
            if (e.Player.InGameId == player2.InGameId)
                turnAdvancedToNextPlayer.TrySetResult();
        };
        var expectedEntry = player1.Results!.First(f => f.ScoreType != Scores.Bonus);

        var completed = await Task.WhenAny(turnAdvancedToNextPlayer.Task, Task.Delay(5000));
        completed.ShouldBe(turnAdvancedToNextPlayer.Task);

        expectedEntry.HasValue.ShouldBeTrue();
        expectedEntry.Value.ShouldBe(0);
        _sut.CurrentPlayer.InGameId.ShouldBe(player2.InGameId);
    }

    [Fact]
    public async Task ServerGameManualApplyScoreBeforeTimeoutPreventsAutoFillForThatTurn()
    {
        _sut = new YatzyServerGame { RoundTimeout = TimeSpan.FromMilliseconds(500) };
        var player1 = new Player(PlayerType.Local);
        var player2 = new Player(PlayerType.Local);
        StartGame(player1);
        StartGame(player2);
        var appliedResults = new List<RollResultEventArgs>();
        _sut.ResultApplied += (_, e) => appliedResults.Add(e);
        var nextRoundStarted = new TaskCompletionSource();
        _sut.TurnChanged += (_, e) =>
        {
            if (e.Move == 2)
                nextRoundStarted.TrySetResult();
        };
        var onesResult = player1.Results!.First(f => f.ScoreType == Scores.Ones);
        onesResult.PossibleValue = 5;

        _sut.ApplyScore(onesResult);

        appliedResults.Count.ShouldBe(1);
        onesResult.Value.ShouldBe(5);

        var completed = await Task.WhenAny(nextRoundStarted.Task, Task.Delay(5000));
        completed.ShouldBe(nextRoundStarted.Task);

        appliedResults.Count.ShouldBe(2);
        appliedResults[0].Player.InGameId.ShouldBe(player1.InGameId);
        appliedResults[0].ScoreType.ShouldBe(Scores.Ones);
        appliedResults[1].Player.InGameId.ShouldBe(player2.InGameId);
    }
}