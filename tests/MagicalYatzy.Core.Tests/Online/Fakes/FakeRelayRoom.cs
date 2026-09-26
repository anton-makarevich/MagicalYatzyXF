using System.Collections.Generic;
using System.Linq;
using Sanet.Transport;

namespace MagicalYatzy.Core.Tests.Online.Fakes;

/// <summary>
/// In-memory relay room that synchronously routes messages between the publishers that joined it.
/// </summary>
public sealed class FakeRelayRoom
{
    private readonly object _syncLock = new();
    private readonly List<FakeTransportPublisher> _publishers = [];

    /// <summary>
    /// When true, a published message is also delivered back to the sender.
    /// </summary>
    public bool EchoToSender { get; set; }

    public IReadOnlyList<FakeTransportPublisher> Publishers
    {
        get
        {
            lock (_syncLock)
            {
                return _publishers.ToArray();
            }
        }
    }

    public FakeTransportPublisher Join(FakeTransportPublisher publisher)
    {
        lock (_syncLock)
        {
            _publishers.Add(publisher);
        }

        publisher.JoinRoom(this);
        return publisher;
    }

    internal void Deliver(FakeTransportPublisher sender, TransportMessage message)
    {
        FakeTransportPublisher[] recipients;
        lock (_syncLock)
        {
            recipients = _publishers
                .Where(p => EchoToSender || p != sender)
                .ToArray();
        }

        foreach (var recipient in recipients)
        {
            recipient.Receive(message);
        }
    }
}
