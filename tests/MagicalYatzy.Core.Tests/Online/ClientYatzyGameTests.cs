using System.Collections.Generic;
using System.Linq;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Online;
using Sanet.MagicalYatzy.Online.Commands;
using Sanet.MagicalYatzy.Online.Commands.Client;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.MagicalYatzy.Online.Commands.Server.State;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.Online;

public class ClientYatzyGameTests
{
    private const string LocalPlayerId = "local-id";
    private const string HostPlayerId = "host-id";
    private const string RemotePlayerId = "remote-id";

    private readonly List<OnlineMessage> _sent = [];

    private ClientYatzyGame CreateSut(string localPlayerId = LocalPlayerId)
    {
        return new ClientYatzyGame(localPlayerId, _sent.Add);
    }

    private static GameState CreateState(
        string gameId = "game-1",
        Rules rule = Rules.krExtended,
        int round = 1,
        bool isPlaying = true,
        string? currentPlayerId = null,
        int[]? lastDice = null,
        int[]? fixedDice = null,
        params PlayerState[] players)
    {
        return new GameState
        {
            GameId = gameId,
            Rule = rule,
            Round = round,
            IsPlaying = isPlaying,
            CurrentPlayerId = currentPlayerId ?? players.FirstOrDefault()?.PlayerId ?? string.Empty,
            LastDiceValues = lastDice ?? [],
            FixedDiceValues = fixedDice ?? [],
            Players = players.ToList()
        };
    }

    private static PlayerState CreatePlayerState(
        string playerId,
        string name = "Player",
        PlayerType type = PlayerType.Network,
        int seatNo = 0,
        bool isReady = true,
        int roll = 1,
        List<ScoreState>? scores = null,
        List<ArtifactState>? artifacts = null)
    {
        return new PlayerState
        {
            PlayerId = playerId,
            Name = name,
            SeatNo = seatNo,
            Type = type,
            IsReady = isReady,
            Roll = roll,
            Total = 0,
            Scores = scores ?? [],
            Artifacts = artifacts ?? []
        };
    }

    private GameState CreateHydratedState(
        string localPlayerId = LocalPlayerId,
        string currentPlayerId = LocalPlayerId,
        Rules rule = Rules.krExtended,
        int[]? fixedDice = null)
    {
        return CreateState(
            currentPlayerId: currentPlayerId,
            rule: rule,
            lastDice: [1, 1, 1, 4, 5],
            fixedDice: fixedDice,
            players:
            [
                CreatePlayerState(HostPlayerId, "Host", PlayerType.Local, seatNo: 0),
                CreatePlayerState(localPlayerId, "Me", PlayerType.Network, seatNo: 1)
            ]);
    }

    [Fact]
    public void Hydration_PreservesServerIdsSeatOrderAndMappedSeatTypes()
    {
        var sut = CreateSut();

        sut.Hydrate(CreateHydratedState());

        sut.GameId.ShouldBe("game-1");
        sut.Rules.CurrentRule.ShouldBe(Rules.krExtended);
        sut.Round.ShouldBe(1);
        sut.IsPlaying.ShouldBeTrue();
        sut.Players.Count.ShouldBe(2);

        var hostSeat = sut.Players[0];
        hostSeat.InGameId.ShouldBe(HostPlayerId);
        // the host's local seat is network from the guest's perspective
        hostSeat.Type.ShouldBe(PlayerType.Network);
        hostSeat.Name.ShouldBe("Host");
        hostSeat.SeatNo.ShouldBe(0);

        var localSeat = sut.Players[1];
        localSeat.InGameId.ShouldBe(LocalPlayerId);
        localSeat.Type.ShouldBe(PlayerType.Local);
        localSeat.SeatNo.ShouldBe(1);

        sut.CurrentPlayer.ShouldBe(localSeat);
        localSeat.IsMyTurn.ShouldBeTrue();
        hostSeat.IsMyTurn.ShouldBeFalse();
    }

    [Fact]
    public void Hydration_DistinguishesCommittedZeroFromUnfilledScore()
    {
        var sut = CreateSut();
        var state = CreateHydratedState();
        state.Players[1] = CreatePlayerState(LocalPlayerId, scores:
        [
            new ScoreState { ScoreType = Scores.Ones, Value = 0, HasValue = true, HasBonus = false },
            new ScoreState { ScoreType = Scores.Twos, Value = 0, HasValue = false, HasBonus = false }
        ]);

        sut.Hydrate(state);

        var ones = sut.Players[1].GetResultForScore(Scores.Ones)!;
        ones.HasValue.ShouldBeTrue();
        ones.Value.ShouldBe(0);

        var twos = sut.Players[1].GetResultForScore(Scores.Twos)!;
        twos.HasValue.ShouldBeFalse();
        twos.Value.ShouldBe(0);
    }

    [Fact]
    public void Hydration_PreservesCommittedValuesBonusesAndUsedArtifacts()
    {
        var sut = CreateSut();
        var state = CreateHydratedState(rule: Rules.krMagic);
        state.Players[1] = CreatePlayerState(LocalPlayerId,
            scores: [new ScoreState { ScoreType = Scores.Kniffel, Value = 50, HasValue = true, HasBonus = false }],
            artifacts:
            [
                new ArtifactState { Type = Artifacts.RollReset, IsUsed = true },
                new ArtifactState { Type = Artifacts.MagicalRoll, IsUsed = false }
            ]);

        sut.Hydrate(state);

        var localSeat = sut.Players[1];
        var kniffel = localSeat.GetResultForScore(Scores.Kniffel)!;
        kniffel.HasValue.ShouldBeTrue();
        kniffel.Value.ShouldBe(50);

        localSeat.MagicalArtifactsForGame.Count.ShouldBe(2);
        localSeat.MagicalArtifactsForGame.Single(a => a.Type == Artifacts.RollReset).IsUsed.ShouldBeTrue();
        localSeat.MagicalArtifactsForGame.Single(a => a.Type == Artifacts.MagicalRoll).IsUsed.ShouldBeFalse();
    }

    [Fact]
    public void Hydration_KeepsDuplicateFixedDice()
    {
        var sut = CreateSut();

        sut.Hydrate(CreateHydratedState(fixedDice: [5, 5]));

        sut.FixedRollResults.ShouldBe([5, 5]);
        sut.NumberOfFixedDice.ShouldBe(2);
        sut.IsDiceFixed(5).ShouldBeTrue();
    }

    [Fact]
    public void Hydration_RecalculatesPossibleValuesOnlyForRolledCurrentPlayer()
    {
        var rolled = CreateSut();
        rolled.Hydrate(CreateHydratedState());
        rolled.CurrentPlayer!.Roll.ShouldBe(1);
        // hydration with a current player that has not rolled keeps zero possible values
        rolled.CurrentPlayer!.GetResultForScore(Scores.Ones)!.PossibleValue.ShouldBe(0);

        var notRolled = CreateSut();
        var state = CreateHydratedState();
        state.Players[1] = CreatePlayerState(LocalPlayerId, roll: 2);
        notRolled.Hydrate(state);
        // dice [1,1,1,4,5] give three ones for the rolled current player
        notRolled.CurrentPlayer!.GetResultForScore(Scores.Ones)!.PossibleValue.ShouldBe(3);
    }

    [Fact]
    public void Hydration_RaisesGameUpdatedOnly()
    {
        var sut = CreateSut();
        var raised = new List<string>();
        sut.GameUpdated += (_, _) => raised.Add(nameof(sut.GameUpdated));
        sut.TurnChanged += (_, _) => raised.Add(nameof(sut.TurnChanged));
        sut.DiceRolled += (_, _) => raised.Add(nameof(sut.DiceRolled));

        sut.Hydrate(CreateHydratedState());
        sut.Hydrate(CreateHydratedState());

        raised.ShouldBe([nameof(sut.GameUpdated), nameof(sut.GameUpdated)]);
    }

    [Fact]
    public void PlayerJoinedBroadcast_AddsUnknownPlayerAndRaisesEvent()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var joinedPlayers = new List<IPlayer>();
        sut.PlayerJoined += (_, e) => joinedPlayers.Add(e.Player);

        sut.ApplyBroadcast(new PlayerJoinedBroadcast
        {
            PlayerId = RemotePlayerId,
            Name = "Remote",
            SeatNo = 2,
            Type = PlayerType.Network
        });

        var added = sut.Players.Single(p => p.InGameId == RemotePlayerId);
        added.Name.ShouldBe("Remote");
        added.SeatNo.ShouldBe(2);
        added.Results.ShouldNotBeNull();
        joinedPlayers.ShouldHaveSingleItem();
        joinedPlayers.Single().ShouldBe(added);

        // a repeated join for a known player does not duplicate the seat
        sut.ApplyBroadcast(new PlayerJoinedBroadcast { PlayerId = RemotePlayerId, Name = "Remote" });
        sut.Players.Count(p => p.InGameId == RemotePlayerId).ShouldBe(1);
    }

    [Fact]
    public void PlayerLeftBroadcast_RemovesPlayerAndRaisesEvent()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var leftPlayers = new List<IPlayer>();
        sut.PlayerLeft += (_, e) => leftPlayers.Add(e.Player);

        sut.ApplyBroadcast(new PlayerLeftBroadcast { PlayerId = HostPlayerId });

        sut.Players.Select(p => p.InGameId).ShouldBe([LocalPlayerId]);
        leftPlayers.ShouldHaveSingleItem();
        leftPlayers.Single().InGameId.ShouldBe(HostPlayerId);
    }

    [Fact]
    public void PlayerReadyBroadcast_SetsReadyStateAndRaisesEvent()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var readyPlayers = new List<IPlayer>();
        sut.PlayerReady += (_, e) => readyPlayers.Add(e.Player);
        sut.Players[0].IsReady.ShouldBeTrue();

        sut.ApplyBroadcast(new PlayerReadyBroadcast { PlayerId = HostPlayerId, IsReady = false });

        sut.Players[0].IsReady.ShouldBeFalse();
        readyPlayers.ShouldHaveSingleItem();
        readyPlayers.Single().InGameId.ShouldBe(HostPlayerId);
    }

    [Fact]
    public void StyleChangedBroadcast_SetsStyleAndRaisesEvent()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var stylePlayers = new List<IPlayer>();
        sut.StyleChanged += (_, e) => stylePlayers.Add(e.Player);

        sut.ApplyBroadcast(new StyleChangedBroadcast { PlayerId = HostPlayerId, Style = DiceStyle.Blue });

        sut.Players[0].SelectedStyle.ShouldBe(DiceStyle.Blue);
        stylePlayers.ShouldHaveSingleItem();
    }

    [Fact]
    public void TurnChangedBroadcast_SetsTurnStateClearsDiceAndRaisesEvent()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState(currentPlayerId: HostPlayerId, fixedDice: [5]));
        sut.IsPlaying = false;
        var turns = new List<MoveEventArgs>();
        sut.TurnChanged += (_, e) => turns.Add(e);

        sut.ApplyBroadcast(new TurnChangedBroadcast { PlayerId = HostPlayerId, Round = 3 });

        sut.CurrentPlayer!.InGameId.ShouldBe(HostPlayerId);
        sut.Round.ShouldBe(3);
        sut.IsPlaying.ShouldBeTrue();
        sut.FixedRollResults.ShouldBeEmpty();
        sut.CurrentPlayer.IsMyTurn.ShouldBeTrue();
        sut.Players[1].IsMyTurn.ShouldBeFalse();
        // values from the earlier turn were dropped
        sut.CurrentPlayer.GetResultForScore(Scores.Ones)!.PossibleValue.ShouldBe(0);
        turns.ShouldHaveSingleItem();
        turns.Single().Move.ShouldBe(3);
        turns.Single().Player.InGameId.ShouldBe(HostPlayerId);
    }

    [Fact]
    public void DiceRolledBroadcast_UpdatesDiceRaisesEventAndConsumesARoll()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var rolls = new List<int[]>();
        sut.DiceRolled += (_, e) => rolls.Add(e.Value);

        sut.ApplyBroadcast(new DiceRolledBroadcast { PlayerId = LocalPlayerId, Values = [2, 2, 3, 4, 5] });

        sut.LastDiceResult.DiceResults.ShouldBe([2, 2, 3, 4, 5]);
        rolls.ShouldHaveSingleItem();
        rolls.Single().ShouldBe([2, 2, 3, 4, 5]);
        sut.CurrentPlayer!.Roll.ShouldBe(2);
        sut.CurrentPlayer.GetResultForScore(Scores.Twos)!.PossibleValue.ShouldBe(4);
    }

    [Fact]
    public void MagicRollUsedBroadcast_DoesNotConsumeARollForThePairedDiceRolledBroadcast()
    {
        var sut = CreateSut();
        var state = CreateHydratedState(rule: Rules.krMagic);
        state.Players[1] = CreatePlayerState(
            LocalPlayerId,
            artifacts: [new ArtifactState { Type = Artifacts.MagicalRoll, IsUsed = false }]);
        sut.Hydrate(state);
        var magicRolls = new List<IPlayer>();
        sut.MagicRollUsed += (_, e) => magicRolls.Add(e.Player);

        sut.ApplyBroadcast(new MagicRollUsedBroadcast { PlayerId = LocalPlayerId, Values = [1, 1, 1, 1, 1] });
        // the host publishes the magic roll as MagicRollUsedBroadcast followed by DiceRolledBroadcast
        sut.ApplyBroadcast(new DiceRolledBroadcast { PlayerId = LocalPlayerId, Values = [1, 1, 1, 1, 1] });

        sut.LastDiceResult.DiceResults.ShouldBe([1, 1, 1, 1, 1]);
        sut.Players[1].MagicalArtifactsForGame.Single(a => a.Type == Artifacts.MagicalRoll).IsUsed
            .ShouldBeTrue();
        magicRolls.ShouldHaveSingleItem();
        // the host does not consume a roll for a magic roll
        sut.CurrentPlayer!.Roll.ShouldBe(1);

        // a regular roll afterwards consumes a roll as usual
        sut.ApplyBroadcast(new DiceRolledBroadcast { PlayerId = LocalPlayerId, Values = [1, 1, 1, 1, 1] });
        sut.CurrentPlayer.Roll.ShouldBe(2);
    }

    [Fact]
    public void DiceChangedBroadcast_UpdatesDiceAndRaisesEvent()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var changes = new List<int[]>();
        sut.DiceChanged += (_, e) => changes.Add(e.Value);

        sut.ApplyBroadcast(new DiceChangedBroadcast
        {
            PlayerId = LocalPlayerId,
            Values = [2, 1, 1, 4, 5],
            OldValue = 2,
            NewValue = 1,
            IsFixed = false
        });

        sut.LastDiceResult.DiceResults.ShouldBe([2, 1, 1, 4, 5]);
        changes.ShouldHaveSingleItem();
        changes.Single().ShouldBe([2, 1, 1, 4, 5]);
    }

    [Fact]
    public void DiceFixedBroadcast_ChangesOneMultisetOccurrencePerBroadcast()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var fixes = new List<FixDiceEventArgs>();
        sut.DiceFixed += (_, e) => fixes.Add(e);

        sut.ApplyBroadcast(new DiceFixedBroadcast { PlayerId = LocalPlayerId, Value = 1, IsFixed = true });
        sut.ApplyBroadcast(new DiceFixedBroadcast { PlayerId = LocalPlayerId, Value = 1, IsFixed = true });
        sut.FixedRollResults.ShouldBe([1, 1]);

        sut.ApplyBroadcast(new DiceFixedBroadcast { PlayerId = LocalPlayerId, Value = 1, IsFixed = false });
        sut.FixedRollResults.ShouldBe([1]);
        sut.IsDiceFixed(1).ShouldBeTrue();

        fixes.Count.ShouldBe(3);
        fixes[0].Value.ShouldBe(1);
        fixes[0].Isfixed.ShouldBeTrue();
        fixes[2].Isfixed.ShouldBeFalse();
    }

    [Fact]
    public void PlayerRerolledBroadcast_ResetsRollClearsFixedDiceAndRaisesEvent()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState(fixedDice: [5, 5]));
        var rerolls = new List<IPlayer>();
        sut.PlayerRerolled += (_, e) => rerolls.Add(e.Player);

        sut.ApplyBroadcast(new PlayerRerolledBroadcast { PlayerId = LocalPlayerId });

        sut.CurrentPlayer!.Roll.ShouldBe(1);
        sut.ReRollMode.ShouldBeTrue();
        sut.FixedRollResults.ShouldBeEmpty();
        rerolls.ShouldHaveSingleItem();
    }

    [Fact]
    public void ScoreAppliedBroadcast_CommitsValueAndBonusBeforeRaisingEvent()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var applied = new List<RollResultEventArgs>();
        sut.ResultApplied += (_, e) => applied.Add(e);

        sut.ApplyBroadcast(new ScoreAppliedBroadcast
        {
            PlayerId = HostPlayerId,
            ScoreType = Scores.Ones,
            Value = 3,
            HasBonus = false
        });
        // an auto-applied numeric bonus arrives as a separate broadcast
        // (the host commits it as the bonus score's MaxValue)
        sut.ApplyBroadcast(new ScoreAppliedBroadcast
        {
            PlayerId = HostPlayerId,
            ScoreType = Scores.Bonus,
            Value = 35,
            HasBonus = false
        });

        var hostSeat = sut.Players[0];
        hostSeat.GetResultForScore(Scores.Ones)!.HasValue.ShouldBeTrue();
        hostSeat.GetResultForScore(Scores.Ones)!.Value.ShouldBe(3);
        hostSeat.GetResultForScore(Scores.Bonus)!.Value.ShouldBe(35);
        applied.Count.ShouldBe(2);
        applied[0].Player.InGameId.ShouldBe(HostPlayerId);
        applied[0].ScoreType.ShouldBe(Scores.Ones);
        applied[0].Value.ShouldBe(3);
        applied[1].ScoreType.ShouldBe(Scores.Bonus);
    }

    [Fact]
    public void ScoreAppliedBroadcast_PreservesCommittedZero()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());

        sut.ApplyBroadcast(new ScoreAppliedBroadcast
        {
            PlayerId = LocalPlayerId,
            ScoreType = Scores.Ones,
            Value = 0,
            HasBonus = false
        });

        var ones = sut.Players[1].GetResultForScore(Scores.Ones)!;
        ones.HasValue.ShouldBeTrue();
        ones.Value.ShouldBe(0);
    }

    [Fact]
    public void GameFinishedBroadcast_OrdersPlayersByStandingsAndRaisesGameFinishedOnce()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var finished = 0;
        sut.GameFinished += (_, _) => finished++;

        sut.ApplyBroadcast(new GameFinishedBroadcast
        {
            Standings =
            [
                new PlayerStanding { PlayerId = LocalPlayerId, Total = 200 },
                new PlayerStanding { PlayerId = HostPlayerId, Total = 100 }
            ]
        });

        sut.IsPlaying.ShouldBeFalse();
        sut.Players.Select(p => p.InGameId).ShouldBe([LocalPlayerId, HostPlayerId]);
        sut.CurrentPlayer!.InGameId.ShouldBe(LocalPlayerId);
        sut.Players.Select(p => p.IsReady).ShouldNotContain(true);
        finished.ShouldBe(1);

        // a repeated broadcast is ignored - the game already finished
        sut.ApplyBroadcast(new GameFinishedBroadcast { Standings = [] });
        finished.ShouldBe(1);
    }

    [Fact]
    public void EndGameLocally_RaisesGameFinishedOnceOnlyIfNotAlreadyFinished()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var finished = 0;
        sut.GameFinished += (_, _) => finished++;

        sut.EndGameLocally();
        sut.IsPlaying.ShouldBeFalse();
        finished.ShouldBe(1);

        sut.EndGameLocally();
        finished.ShouldBe(1);

        // after a host-announced finish the local end is a no-op
        var announced = CreateSut();
        announced.Hydrate(CreateHydratedState());
        var announcedFinished = 0;
        announced.GameFinished += (_, _) => announcedFinished++;
        announced.ApplyBroadcast(new GameFinishedBroadcast
        {
            Standings = [new PlayerStanding { PlayerId = LocalPlayerId, Total = 0 }]
        });
        announced.EndGameLocally();
        announcedFinished.ShouldBe(1);
    }

    [Fact]
    public void BroadcastsForUnknownPlayersAreIgnored()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var raised = new List<string>();
        sut.PlayerLeft += (_, _) => raised.Add("left");
        sut.PlayerReady += (_, _) => raised.Add("ready");
        sut.TurnChanged += (_, _) => raised.Add("turn");
        sut.DiceRolled += (_, _) => raised.Add("rolled");

        sut.ApplyBroadcast(new PlayerLeftBroadcast { PlayerId = "unknown" });
        sut.ApplyBroadcast(new PlayerReadyBroadcast { PlayerId = "unknown", IsReady = true });
        sut.ApplyBroadcast(new TurnChangedBroadcast { PlayerId = "unknown", Round = 2 });
        sut.ApplyBroadcast(new DiceRolledBroadcast { PlayerId = "unknown", Values = [1, 2, 3, 4, 5] });

        raised.ShouldBeEmpty();
        sut.Players.Count.ShouldBe(2);
    }

    [Fact]
    public void GameStateBroadcast_RehydratesProjection()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());

        sut.ApplyBroadcast(new GameStateBroadcast
        {
            State = CreateState(
                gameId: "game-1",
                round: 5,
                currentPlayerId: HostPlayerId,
                lastDice: [6, 6, 6, 6, 6],
                players:
                [
                    CreatePlayerState(HostPlayerId, "Host", PlayerType.Local, seatNo: 0),
                    CreatePlayerState(LocalPlayerId, "Me", PlayerType.Network, seatNo: 1)
                ])
        });

        sut.Round.ShouldBe(5);
        sut.CurrentPlayer!.InGameId.ShouldBe(HostPlayerId);
        sut.LastDiceResult.DiceResults.ShouldBe([6, 6, 6, 6, 6]);
    }

    [Fact]
    public void TurnIntents_SendCommandsWithLocalPlayerIdWhenLocalPlayerHoldsTurn()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());

        sut.ReportRoll();
        sut.ReportMagicRoll();
        sut.ResetRolls();
        sut.FixDice(1, true);
        sut.FixAllDice(1, false);
        sut.ManualChange(2, 3, true);
        sut.ApplyScore(sut.CurrentPlayer!.GetResultForScore(Scores.Ones)!);

        _sent.OfType<RollCommand>().Single().PlayerId.ShouldBe(LocalPlayerId);
        _sent.OfType<MagicRollCommand>().Single().PlayerId.ShouldBe(LocalPlayerId);
        _sent.OfType<ResetRollsCommand>().Single().PlayerId.ShouldBe(LocalPlayerId);
        _sent.OfType<FixDiceCommand>().Single().ShouldSatisfyAllConditions(
            c => c.PlayerId.ShouldBe(LocalPlayerId),
            c => c.Value.ShouldBe(1),
            c => c.IsFixed.ShouldBeTrue());
        _sent.OfType<FixAllDiceCommand>().Single().ShouldSatisfyAllConditions(
            c => c.PlayerId.ShouldBe(LocalPlayerId),
            c => c.Value.ShouldBe(1),
            c => c.IsFixed.ShouldBeFalse());
        _sent.OfType<ManualChangeCommand>().Single().ShouldSatisfyAllConditions(
            c => c.PlayerId.ShouldBe(LocalPlayerId),
            c => c.OldValue.ShouldBe(2),
            c => c.NewValue.ShouldBe(3),
            c => c.IsFixed.ShouldBeTrue());
        _sent.OfType<ApplyScoreCommand>().Single().ShouldSatisfyAllConditions(
            c => c.PlayerId.ShouldBe(LocalPlayerId),
            c => c.ScoreType.ShouldBe(Scores.Ones));

        // intents never change projected state - only broadcasts do
        sut.CurrentPlayer!.Roll.ShouldBe(1);
        sut.FixedRollResults.ShouldBeEmpty();
        sut.LastDiceResult.DiceResults.ShouldBe([1, 1, 1, 4, 5]);
    }

    [Fact]
    public void TurnIntents_AreNotSentWhenAnotherPlayerHoldsTurn()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState(currentPlayerId: HostPlayerId));

        sut.ReportRoll();
        sut.ReportMagicRoll();
        sut.ResetRolls();
        sut.FixDice(1, true);
        sut.FixAllDice(1, true);
        sut.ManualChange(2, 3, false);
        sut.ApplyScore(sut.Players[0].GetResultForScore(Scores.Ones)!);

        _sent.ShouldBeEmpty();
    }

    [Fact]
    public void ReadyStyleAndLeaveIntents_AreSentOnlyForTheLocalPlayer()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState(currentPlayerId: HostPlayerId));
        var localSeat = sut.Players.Single(p => p.InGameId == LocalPlayerId);
        var hostSeat = sut.Players.Single(p => p.InGameId == HostPlayerId);

        sut.SetPlayerReady(localSeat, true);
        sut.ChangeStyle(localSeat, DiceStyle.Red);
        sut.LeaveGame(localSeat);
        _sent.Count.ShouldBe(3);
        _sent.OfType<ReadyCommand>().Single().IsReady.ShouldBeTrue();
        _sent.OfType<ChangeStyleCommand>().Single().Style.ShouldBe(DiceStyle.Red);
        _sent.OfType<LeaveGameCommand>().ShouldHaveSingleItem();

        _sent.Clear();
        sut.SetPlayerReady(hostSeat, true);
        sut.ChangeStyle(hostSeat, DiceStyle.Red);
        sut.LeaveGame(hostSeat);
        _sent.ShouldBeEmpty();
    }

    [Fact]
    public void HostOwnedOperations_AreNoOpsWithoutCommandsOrEvents()
    {
        var sut = CreateSut();
        sut.Hydrate(CreateHydratedState());
        var raised = new List<string>();
        sut.PlayerJoined += (_, _) => raised.Add("joined");
        sut.ChatMessageSent += (_, _) => raised.Add("chat");

        sut.JoinGame(new Player(PlayerType.Local, "Someone"));
        sut.DoTurn();
        sut.NextTurn();
        sut.RestartGame();
        sut.SendChatMessage(new Sanet.MagicalYatzy.Models.Chat.ChatMessage());

        raised.ShouldBeEmpty();
        _sent.ShouldBeEmpty();
    }
}
