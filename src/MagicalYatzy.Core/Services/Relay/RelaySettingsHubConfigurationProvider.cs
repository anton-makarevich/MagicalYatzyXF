using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanet.Transport.SignalR.Client.Relay;

namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// Adapts the app's single in-memory relay settings entry to the transport package's hub provider contract.
/// </summary>
public sealed class RelaySettingsHubConfigurationProvider(IRelaySettings relaySettings)
    : IRelayHubConfigurationProvider
{
    private const string HubId = "default";
    private const string HubName = "Relay Hub";

    public Task<RelayClientOptions> GetActiveOptions() => Task.FromResult(new RelayClientOptions
    {
        BaseUrl = relaySettings.BaseUrl,
        ApiKey = relaySettings.ApiKey
    });

    public Task<string> GetActiveHubId() => Task.FromResult(HubId);

    public Task<IReadOnlyList<HubConfigData>> GetHubs() => Task.FromResult<IReadOnlyList<HubConfigData>>(
        [new HubConfigData(HubId, HubName, relaySettings.BaseUrl, relaySettings.ApiKey, IsBuiltIn: false)]);

    public Task AddHub(HubConfigData hub) =>
        throw new NotSupportedException("Only the single settings-backed relay hub is supported.");

    public Task UpdateHub(string id, string name, string baseUrl, string apiKey)
    {
        EnsureDefaultHub(id);
        relaySettings.BaseUrl = baseUrl;
        relaySettings.ApiKey = apiKey;
        return Task.CompletedTask;
    }

    public Task RemoveHub(string id) =>
        throw new NotSupportedException("The settings-backed relay hub cannot be removed.");

    public Task SelectHub(string id)
    {
        EnsureDefaultHub(id);
        return Task.CompletedTask;
    }

    private static void EnsureDefaultHub(string id)
    {
        if (id != HubId)
        {
            throw new ArgumentException($"Hub '{id}' does not exist.", nameof(id));
        }
    }
}