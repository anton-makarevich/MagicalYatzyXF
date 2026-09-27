using System.Threading;
using System.Threading.Tasks;
using Sanet.Transport;

namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// Creates a connected relay-hub transport publisher for a hosted room. A seam so tests can
/// substitute an in-memory publisher while production code uses the SignalR relay transport.
/// </summary>
public interface IRelayPublisherProvider
{
    Task<ITransportPublisher> Create(
        string roomCode,
        string relayTicket,
        CancellationToken cancellationToken = default);
}
