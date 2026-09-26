using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanet.Transport;

namespace MagicalYatzy.Core.Tests.Online.Fakes;

/// <summary>
/// Test-only in-memory transport publisher. Delivers published messages to the other publishers of
/// the room it joined and records every published message for assertions.
/// </summary>
public sealed class FakeTransportPublisher : ITransportPublisher
{
    private FakeRelayRoom? _room;
    private Action<TransportMessage>? _onMessageReceived;
    private TransportConnectionState _connectionState = TransportConnectionState.Connected;

    public TransportConnectionState ConnectionState => _connectionState;

    public event Action<TransportConnectionState>? ConnectionStateChanged;

    private List<TransportMessage> PublishedMessages { get; } = [];

    public void JoinRoom(FakeRelayRoom room)
    {
        _room = room;
    }

    public Task PublishMessage(TransportMessage message)
    {
        PublishedMessages.Add(message);
        _room?.Deliver(this, message);
        return Task.CompletedTask;
    }

    public void Subscribe(Action<TransportMessage> onMessageReceived)
    {
        _onMessageReceived = onMessageReceived;
    }

    /// <summary>
    /// Delivers a message as if it arrived from the relay room.
    /// </summary>
    public void Receive(TransportMessage message)
    {
        _onMessageReceived?.Invoke(message);
    }

    /// <summary>
    /// Sets the connection state and raises <see cref="ConnectionStateChanged"/>.
    /// </summary>
    public void SetConnectionState(TransportConnectionState state)
    {
        if (_connectionState == state)
        {
            return;
        }

        _connectionState = state;
        ConnectionStateChanged?.Invoke(state);
    }

    public ValueTask DisposeAsync()
    {
        _room = null;
        _onMessageReceived = null;
        return ValueTask.CompletedTask;
    }
}
