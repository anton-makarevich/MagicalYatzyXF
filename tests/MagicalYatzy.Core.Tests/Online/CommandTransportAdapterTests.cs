using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MagicalYatzy.Core.Tests.Online.Fakes;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Online;
using Sanet.MagicalYatzy.Online.Commands;
using Sanet.MagicalYatzy.Online.Commands.Client;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.Transport;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.Online;

public class CommandTransportAdapterTests
{
    private readonly CommandRegistry _registry = new();
    private readonly FakeRelayRoom _room = new();

    private (CommandTransportAdapter Adapter, FakeTransportPublisher Publisher) CreateAdapter()
    {
        var publisher = new FakeTransportPublisher();
        _room.Join(publisher);
        var adapter = new CommandTransportAdapter(_registry);
        adapter.AddPublisher(publisher);
        return (adapter, publisher);
    }

    [Fact]
    public async Task ClientCommand_DispatchesToOtherAdapter()
    {
        var (adapterA, _) = CreateAdapter();
        var (adapterB, _) = CreateAdapter();

        OnlineMessage? received = null;
        adapterB.Initialize(message => received = message);

        var command = new FixDiceCommand { PlayerId = "p1", Value = 4, IsFixed = true };

        await adapterA.PublishMessage(command);

        received.ShouldNotBeNull();
        received.ShouldBeOfType<FixDiceCommand>();
        received.ShouldBeEquivalentTo(command);
    }

    [Fact]
    public async Task ServerBroadcast_DispatchesToOtherAdapter()
    {
        var (adapterA, _) = CreateAdapter();
        var (adapterB, _) = CreateAdapter();

        OnlineMessage? received = null;
        adapterA.Initialize(message => received = message);

        var broadcast = new ScoreAppliedBroadcast { PlayerId = "p1", ScoreType = Scores.Kniffel, Value = 50, HasBonus = true };

        await adapterB.PublishMessage(broadcast);

        received.ShouldNotBeNull();
        received.ShouldBeOfType<ScoreAppliedBroadcast>();
        received.ShouldBeEquivalentTo(broadcast);
    }

    [Fact]
    public void PublisherAddedAfterInitialize_DeliversMessages()
    {
        var p1 = new FakeTransportPublisher();
        _room.Join(p1);
        var adapter = new CommandTransportAdapter(_registry);
        adapter.AddPublisher(p1);

        var received = new List<OnlineMessage>();
        adapter.Initialize(received.Add);

        var p2 = new FakeTransportPublisher();
        _room.Join(p2);
        adapter.AddPublisher(p2);

        var message = _registry.ToTransportMessage(new RollCommand { PlayerId = "p1" }, Guid.NewGuid());
        p2.Receive(message);

        received.ShouldHaveSingleItem();
        received[0].ShouldBeOfType<RollCommand>();
    }

    [Fact]
    public async Task SelfEcho_DoesNotInvokeSenderCallback()
    {
        var (adapter, _) = CreateAdapter();
        _room.EchoToSender = true;

        OnlineMessage? received = null;
        adapter.Initialize(message => received = message);

        await adapter.PublishMessage(new RollCommand { PlayerId = "p1" });

        received.ShouldBeNull();
    }

    [Fact]
    public void UnknownMessageType_DoesNotThrowOrInvokeCallback()
    {
        var (adapter, publisher) = CreateAdapter();

        OnlineMessage? received = null;
        adapter.Initialize(message => received = message);

        var message = new TransportMessage
        {
            MessageType = "NoSuchMessage",
            SourceId = Guid.NewGuid(),
            Payload = "{}"
        };

        Should.NotThrow(() => publisher.Receive(message));
        received.ShouldBeNull();
    }

    [Fact]
    public void MalformedPayload_DoesNotThrowOrInvokeCallback()
    {
        var (adapter, publisher) = CreateAdapter();

        OnlineMessage? received = null;
        adapter.Initialize(message => received = message);

        var message = new TransportMessage
        {
            MessageType = nameof(RollCommand),
            SourceId = Guid.NewGuid(),
            Payload = "{ not valid json"
        };

        Should.NotThrow(() => publisher.Receive(message));
        received.ShouldBeNull();
    }

    [Fact]
    public async Task Dispose_StopsDeliveringMessages()
    {
        var (adapterA, _) = CreateAdapter();
        var (adapterB, _) = CreateAdapter();

        OnlineMessage? received = null;
        adapterA.Initialize(message => received = message);

        adapterA.Dispose();

        await adapterB.PublishMessage(new RollCommand { PlayerId = "p1" });

        received.ShouldBeNull();
    }

    [Fact]
    public void AggregateConnected_WhenAllPublishersConnected()
    {
        var (adapter, _) = CreateAdapter();
        var publisher2 = new FakeTransportPublisher();
        _room.Join(publisher2);
        adapter.AddPublisher(publisher2);

        adapter.ConnectionState.ShouldBe(TransportConnectionState.Connected);
    }

    [Fact]
    public void AggregateChangesAndEventFiresOnce_WhenOnePublisherDisconnects()
    {
        var (adapter, publisher1) = CreateAdapter();
        var publisher2 = new FakeTransportPublisher();
        _room.Join(publisher2);
        adapter.AddPublisher(publisher2);

        var states = new List<TransportConnectionState>();
        adapter.ConnectionStateChanged += states.Add;

        publisher1.SetConnectionState(TransportConnectionState.Disconnected);

        adapter.ConnectionState.ShouldBe(TransportConnectionState.Disconnected);
        states.ShouldBe(new[] { TransportConnectionState.Disconnected });
    }

    [Fact]
    public void HostDisconnectedFiresOnce_OnConnectedToClosed()
    {
        var (adapter, publisher1) = CreateAdapter();
        var publisher2 = new FakeTransportPublisher();
        _room.Join(publisher2);
        adapter.AddPublisher(publisher2);

        var count = 0;
        adapter.HostDisconnected += () => count++;

        publisher1.SetConnectionState(TransportConnectionState.Closed);
        publisher2.SetConnectionState(TransportConnectionState.Closed);

        count.ShouldBe(1);
    }

    [Fact]
    public void ExplicitTrigger_RaisesHostDisconnectedOnce()
    {
        var (adapter, _) = CreateAdapter();

        var count = 0;
        adapter.HostDisconnected += () => count++;

        adapter.NotifyHostDisconnected();
        adapter.NotifyHostDisconnected();

        count.ShouldBe(1);
    }

    [Fact]
    public void HostDisconnected_NotRaisedForTransientReconnecting()
    {
        var (adapter, publisher) = CreateAdapter();

        var count = 0;
        adapter.HostDisconnected += () => count++;

        publisher.SetConnectionState(TransportConnectionState.Reconnecting);

        adapter.ConnectionState.ShouldBe(TransportConnectionState.Reconnecting);
        count.ShouldBe(0);
    }

    [Fact]
    public async Task PublishMessage_WithNoPublishers_CompletesSuccessfully()
    {
        var adapter = new CommandTransportAdapter(_registry);

        await adapter.PublishMessage(new RollCommand { PlayerId = "p1" });
    }

    [Fact]
    public async Task PublishMessage_CallsPublishOnAllPublishers()
    {
        var publisher1 = Substitute.For<ITransportPublisher>();
        var publisher2 = Substitute.For<ITransportPublisher>();
        publisher1.ConnectionState.Returns(TransportConnectionState.Connected);
        publisher2.ConnectionState.Returns(TransportConnectionState.Connected);
        publisher1.PublishMessage(Arg.Any<TransportMessage>()).Returns(Task.CompletedTask);
        publisher2.PublishMessage(Arg.Any<TransportMessage>()).Returns(Task.CompletedTask);

        var adapter = new CommandTransportAdapter(_registry);
        adapter.AddPublisher(publisher1);
        adapter.AddPublisher(publisher2);

        await adapter.PublishMessage(new RollCommand { PlayerId = "p1" });

        await publisher1.Received(1).PublishMessage(Arg.Any<TransportMessage>());
        await publisher2.Received(1).PublishMessage(Arg.Any<TransportMessage>());
    }
}
