using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Online.Commands;
using Sanet.Transport;

namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// Bridges online messages and transport publishers: serializes/publishes outbound messages and
/// deserializes/dispatches inbound messages, while aggregating connection state across publishers.
/// </summary>
public sealed class CommandTransportAdapter : IDisposable
{
    private readonly CommandRegistry _registry;
    private readonly Guid _sourceId;
    private readonly object _syncLock = new();
    private readonly List<ITransportPublisher> _publishers = [];
    private readonly Dictionary<ITransportPublisher, Action<TransportConnectionState>> _connectionStateHandlers = [];
    private readonly Dictionary<ITransportPublisher, TransportConnectionState> _publisherStates = [];

    private Action<OnlineMessage>? _onMessageReceived;
    private bool _isInitialized;
    private bool _isDisposed;
    private bool _hostDisconnectedRaised;
    private TransportConnectionState _connectionState = TransportConnectionState.Disconnected;

    public CommandTransportAdapter(CommandRegistry registry, Guid? sourceId = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _sourceId = sourceId ?? Guid.NewGuid();
    }

    /// <summary>
    /// The unique source id stamped on every outbound message and used to filter self-echoes.
    /// </summary>
    public Guid SourceId => _sourceId;

    /// <summary>
    /// The aggregated connection state: connected only when every registered publisher is connected.
    /// </summary>
    public TransportConnectionState ConnectionState
    {
        get
        {
            lock (_syncLock)
            {
                return _connectionState;
            }
        }
    }

    /// <summary>
    /// Raised when the aggregated connection state changes.
    /// </summary>
    public event Action<TransportConnectionState>? ConnectionStateChanged;

    /// <summary>
    /// Raised once when the aggregated connection transitions from connected to a terminal
    /// disconnected/closed state, or when <see cref="NotifyHostDisconnected"/> is called.
    /// </summary>
    public event Action? HostDisconnected;

    /// <summary>
    /// Registers a publisher. Subscribes its connection state (and its message callback if
    /// <see cref="Initialize"/> already ran).
    /// </summary>
    public void AddPublisher(ITransportPublisher publisher)
    {
        lock (_syncLock)
        {
            if (_isDisposed)
            {
                return;
            }

            if (_publishers.Contains(publisher))
            {
                return;
            }

            _publishers.Add(publisher);
            SubscribeConnectionState(publisher);

            if (_isInitialized)
            {
                SubscribeMessage(publisher);
            }
        }
    }

    /// <summary>
    /// Stores the receive callback and subscribes it on every existing publisher. Only the first
    /// call takes effect.
    /// </summary>
    public void Initialize(Action<OnlineMessage> onMessageReceived)
    {
        ITransportPublisher[] snapshot;
        lock (_syncLock)
        {
            if (_isInitialized)
            {
                return;
            }

            _onMessageReceived = onMessageReceived;
            _isInitialized = true;
            snapshot = _publishers.ToArray();
        }

        foreach (var publisher in snapshot)
        {
            SubscribeMessage(publisher);
        }
    }

    /// <summary>
    /// Serializes and publishes the message to every registered publisher.
    /// </summary>
    public Task PublishMessage(OnlineMessage message)
    {
        var transportMessage = _registry.ToTransportMessage(message, _sourceId);

        ITransportPublisher[] snapshot;
        lock (_syncLock)
        {
            snapshot = _publishers.ToArray();
        }

        if (snapshot.Length == 0)
        {
            return Task.CompletedTask;
        }

        var tasks = new Task[snapshot.Length];
        for (var i = 0; i < snapshot.Length; i++)
        {
            tasks[i] = snapshot[i].PublishMessage(transportMessage);
        }

        return Task.WhenAll(tasks);
    }

    /// <summary>
    /// Raises <see cref="HostDisconnected"/> explicitly, for a later relay-room host-loss signal.
    /// The event is still raised at most once.
    /// </summary>
    public void NotifyHostDisconnected()
    {
        lock (_syncLock)
        {
            RaiseHostDisconnected();
        }
    }

    /// <summary>
    /// Unsubscribes from every publisher's connection-state event and clears the receive callback.
    /// </summary>
    public void Dispose()
    {
        lock (_syncLock)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            foreach (var (publisher, handler) in _connectionStateHandlers)
            {
                publisher.ConnectionStateChanged -= handler;
            }

            _connectionStateHandlers.Clear();
            _publisherStates.Clear();
            _publishers.Clear();
            _onMessageReceived = null;
            _isInitialized = false;
        }
    }

    private void SubscribeMessage(ITransportPublisher publisher)
    {
        publisher.Subscribe(message =>
        {
            Action<OnlineMessage>? callback;
            lock (_syncLock)
            {
                if (_isDisposed)
                {
                    return;
                }

                callback = _onMessageReceived;
            }

            if (callback == null)
            {
                return;
            }

            if (message.SourceId == _sourceId)
            {
                return;
            }

            var onlineMessage = _registry.TryDeserialize(message);
            if (onlineMessage == null)
            {
                return;
            }

            callback(onlineMessage);
        });
    }

    private void SubscribeConnectionState(ITransportPublisher publisher)
    {
        if (_connectionStateHandlers.ContainsKey(publisher))
        {
            return;
        }

        void Handler(TransportConnectionState state)
        {
            lock (_syncLock)
            {
                if (_isDisposed)
                {
                    return;
                }

                if (!_publishers.Contains(publisher))
                {
                    return;
                }

                _publisherStates[publisher] = state;
                RecomputeAggregateState();
            }
        }

        publisher.ConnectionStateChanged += Handler;
        _connectionStateHandlers[publisher] = Handler;

        _publisherStates[publisher] = publisher.ConnectionState;
        RecomputeAggregateState();
    }

    private void RecomputeAggregateState()
    {
        var newState = DeriveConnectionState();
        if (newState == _connectionState)
        {
            return;
        }

        var previous = _connectionState;
        _connectionState = newState;
        ConnectionStateChanged?.Invoke(newState);

        var reachedTerminal = newState == TransportConnectionState.Disconnected
            || newState == TransportConnectionState.Closed;
        if (previous == TransportConnectionState.Connected && reachedTerminal)
        {
            RaiseHostDisconnected();
        }
    }

    private TransportConnectionState DeriveConnectionState()
    {
        var states = _publisherStates.Values;
        if (states.Count == 0)
        {
            return TransportConnectionState.Disconnected;
        }

        if (states.All(s => s == TransportConnectionState.Connected))
        {
            return TransportConnectionState.Connected;
        }

        if (states.Any(s => s == TransportConnectionState.Closed))
        {
            return TransportConnectionState.Closed;
        }

        if (states.Any(s => s == TransportConnectionState.Disconnected))
        {
            return TransportConnectionState.Disconnected;
        }

        if (states.Any(s => s == TransportConnectionState.Reconnecting))
        {
            return TransportConnectionState.Reconnecting;
        }

        return TransportConnectionState.Connecting;
    }

    private void RaiseHostDisconnected()
    {
        if (_hostDisconnectedRaised)
        {
            return;
        }

        _hostDisconnectedRaised = true;
        HostDisconnected?.Invoke();
    }
}
