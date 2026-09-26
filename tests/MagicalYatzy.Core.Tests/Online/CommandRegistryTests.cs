using System;
using System.Collections.Generic;
using System.Linq;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Online;
using Sanet.MagicalYatzy.Online.Commands;
using Sanet.MagicalYatzy.Online.Commands.Client;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.MagicalYatzy.Online.Commands.Server.State;
using Sanet.Transport;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.Online;

public class CommandRegistryTests
{
    public static IEnumerable<object[]> RoundTripSamples()
    {
        yield return [new ReadyCommand { PlayerId = "p1", IsReady = true }];
        yield return [new RollCommand { PlayerId = "p1" }];
        yield return [new FixDiceCommand { PlayerId = "p1", Value = 5, IsFixed = true }];
        yield return [new FixAllDiceCommand { PlayerId = "p1", Value = 3, IsFixed = true }];
        yield return [new MagicRollCommand { PlayerId = "p1" }];
        yield return [new ResetRollsCommand { PlayerId = "p1" }];
        yield return [new ManualChangeCommand { PlayerId = "p1", OldValue = 2, NewValue = 6, IsFixed = true }];
        yield return [new ApplyScoreCommand { PlayerId = "p1", ScoreType = Scores.FullHouse }];
        yield return [new ChangeStyleCommand { PlayerId = "p1", Style = DiceStyle.Red }];
        yield return [new LeaveGameCommand { PlayerId = "p1" }];
        yield return [new RequestGameStateCommand { PlayerId = "p1" }];

        yield return [new PlayerJoinedBroadcast { PlayerId = "p1", Name = "Alice", SeatNo = 1, Type = PlayerType.Network }
        ];
        yield return [new PlayerLeftBroadcast { PlayerId = "p1" }];
        yield return [new PlayerReadyBroadcast { PlayerId = "p1", IsReady = true }];
        yield return [new TurnChangedBroadcast { PlayerId = "p1", Round = 4 }];
        yield return [new DiceRolledBroadcast { PlayerId = "p1", Values = [1, 2, 3, 4, 5] }];
        yield return [new DiceFixedBroadcast { PlayerId = "p1", Value = 6, IsFixed = true, All = true }];
        yield return [new DiceChangedBroadcast { PlayerId = "p1", Values = [2, 3, 4, 5, 6], OldValue = 1, NewValue = 6, IsFixed = true }
        ];
        yield return [new PlayerRerolledBroadcast { PlayerId = "p1" }];
        yield return [new MagicRollUsedBroadcast { PlayerId = "p1", Values = [6, 6, 6, 6, 6] }];
        yield return [new StyleChangedBroadcast { PlayerId = "p1", Style = DiceStyle.Blue }];
        yield return [new ScoreAppliedBroadcast { PlayerId = "p1", ScoreType = Scores.Kniffel, Value = 50, HasBonus = true }
        ];
        yield return [new GameStateBroadcast { PlayerId = string.Empty, State = CreateGameState() }];
        yield return [new GameFinishedBroadcast { PlayerId = string.Empty, Standings =
                [new() { PlayerId = "p1", Total = 300 }, new() { PlayerId = "p2", Total = 250 }]
            }
        ];
        yield return [new GameRestartedBroadcast { PlayerId = string.Empty }];
        yield return [new GameEndedBroadcast { PlayerId = string.Empty, Reason = GameEndReason.HostLeft }];
    }

    [Theory]
    [MemberData(nameof(RoundTripSamples))]
    public void RoundTripsThroughTransportMessage(OnlineMessage sample)
    {
        var registry = new CommandRegistry();
        var sourceId = Guid.NewGuid();

        var transportMessage = registry.ToTransportMessage(sample, sourceId);

        transportMessage.MessageType.ShouldBe(sample.MessageType);
        transportMessage.SourceId.ShouldBe(sourceId);
        transportMessage.Payload.ShouldNotBeNullOrEmpty();

        var deserialized = registry.TryDeserialize(transportMessage);

        deserialized.ShouldNotBeNull();
        deserialized!.GetType().ShouldBe(sample.GetType());
        deserialized.ShouldBeEquivalentTo(sample);
    }

    [Fact]
    public void EveryConcreteOnlineMessageSubclassIsRegistered()
    {
        var registry = new CommandRegistry();
        var messageTypes = typeof(OnlineMessage).Assembly
            .GetTypes()
            .Where(t => typeof(OnlineMessage).IsAssignableFrom(t)
                        && t is { IsAbstract: false, IsInterface: false })
            .ToList();

        messageTypes.ShouldNotBeEmpty();

        foreach (var type in messageTypes)
        {
            registry.GetType(type.Name).ShouldNotBeNull($"type {type.Name} should be registered");
        }
    }

    [Fact]
    public void EveryRegisteredTypeHasARoundTripSample()
    {
        var registry = new CommandRegistry();
        var sampleTypes = RoundTripSamples()
            .Select(sample => ((OnlineMessage)sample[0]).GetType())
            .ToHashSet();

        foreach (var type in registry.Types.Values)
        {
            sampleTypes.ShouldContain(type, $"type {type.Name} should have a round-trip sample");
        }
    }

    [Fact]
    public void UnknownMessageType_ReturnsNull()
    {
        var registry = new CommandRegistry();
        var message = new TransportMessage
        {
            MessageType = "NoSuchMessage",
            SourceId = Guid.NewGuid(),
            Payload = "{}"
        };

        registry.TryDeserialize(message).ShouldBeNull();
    }

    [Fact]
    public void MalformedPayload_ReturnsNull()
    {
        var registry = new CommandRegistry();
        var message = new TransportMessage
        {
            MessageType = nameof(RollCommand),
            SourceId = Guid.NewGuid(),
            Payload = "{ not valid json"
        };

        registry.TryDeserialize(message).ShouldBeNull();
    }

    [Fact]
    public void GameState_PreservesDuplicateFixedDiceAndUnfilledZeroScore()
    {
        var registry = new CommandRegistry();
        var sample = new GameStateBroadcast { State = CreateGameState() };

        var transport = registry.ToTransportMessage(sample, Guid.NewGuid());
        var state = registry.TryDeserialize(transport)!.ShouldBeOfType<GameStateBroadcast>().State;

        state.FixedDiceValues.ShouldBeEquivalentTo(new[] { 4, 4, 6 });
        state.Players.ShouldHaveSingleItem();
        state.Players[0].Scores
            .Any(s => s.ScoreType == Scores.Kniffel && !s.HasValue && s.Value == 0)
            .ShouldBeTrue();
    }

    private static GameState CreateGameState() => new()
    {
        GameId = "game-123",
        Rule = Rules.krMagic,
        Round = 5,
        IsPlaying = true,
        ReRollMode = true,
        CurrentPlayerId = "p1",
        LastDiceValues = [3, 4, 4, 5, 6],
        FixedDiceValues = [4, 4, 6],
        Players =
        [

            new()
            {
                PlayerId = "p1",
                Name = "Alice",
                SeatNo = 1,
                Type = PlayerType.Local,
                IsReady = true,
                Roll = 2,
                SelectedStyle = DiceStyle.Classic,
                Total = 150,
                Scores =
                [
                    new() { ScoreType = Scores.Ones, Value = 3, HasValue = true, HasBonus = false },
                    new() { ScoreType = Scores.Kniffel, Value = 0, HasValue = false, HasBonus = false }
                ],
                Artifacts =
                [
                    new() { Type = Artifacts.MagicalRoll, IsUsed = true },
                    new() { Type = Artifacts.RollReset, IsUsed = false }
                ]
            }
        ]
    };
}
