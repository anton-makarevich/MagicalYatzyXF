using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Sanet.Transport;
using Sanet.Transport.SignalR.Client.Factories;
using Sanet.Transport.SignalR.Client.Relay;

namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// Default <see cref="IRelayPublisherProvider"/>: builds a <see cref="RelayPublisherOptions"/>
/// from the active relay-hub configuration and delegates to the transport package's
/// <see cref="RelayPublisherFactory"/>, which returns an already-connected publisher.
/// </summary>
public sealed class RelayPublisherProvider
    (IRelayHubConfigurationProvider configurationProvider) : IRelayPublisherProvider
{
    public static string ComposeHubUrl(string baseUrl) =>
        RelayHubDefaults.BuildHubUrl(baseUrl);

    public async Task<ITransportPublisher> Create(
        string roomCode,
        string relayTicket,
        CancellationToken cancellationToken = default)
    {
        var options = await configurationProvider.GetActiveOptions();
        var factory = new RelayPublisherFactory(NullLoggerFactory.Instance);
        return await factory.Create(
            new RelayPublisherOptions
            {
                HubUrl = ComposeHubUrl(options.BaseUrl),
                RoomCode = roomCode,
                RelayTicket = relayTicket,
                TicketRefresh = null
            },
            cancellationToken);
    }
}
