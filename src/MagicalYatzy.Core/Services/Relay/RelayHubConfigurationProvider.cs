using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Services.StorageService;
using Sanet.Transport.SignalR.Client.Relay;

namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// Singleton that owns the relay hub configuration. The built-in hub is seeded from
/// <see cref="IRelaySettings"/> while user-defined hubs and the active selection are
/// persisted through <see cref="ISettingsStorageService"/>.
/// </summary>
public sealed class RelayHubConfigurationProvider : IRelayHubConfigurationProvider
{
    private const string StorageKey = "HubConfigurations";
    private const string BuiltInHubId = "default";
    private const string BuiltInHubName = "Relay Hub";

    private readonly ISettingsStorageService _storage;
    private readonly IRelaySettings _relaySettings;
    private readonly Dictionary<string, HubConfigData> _hubs = new(StringComparer.Ordinal);
    private readonly Lock _gate = new();
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private readonly Task _loadTask;
    private string _activeHubId = BuiltInHubId;

    public RelayHubConfigurationProvider(IRelaySettings relaySettings, ISettingsStorageService storage)
    {
        _storage = storage;
        _relaySettings = relaySettings;
        _hubs[BuiltInHubId] = new HubConfigData(
            BuiltInHubId,
            BuiltInHubName,
            relaySettings.BaseUrl,
            relaySettings.ApiKey,
            IsBuiltIn: true);
        _loadTask = LoadAsync();
    }

    public async Task<RelayClientOptions> GetActiveOptions()
    {
        await EnsureLoadedAsync();
        lock (_gate)
        {
            var active = _hubs[_activeHubId];
            return new RelayClientOptions
            {
                BaseUrl = active.IsBuiltIn ? _relaySettings.BaseUrl : active.BaseUrl,
                ApiKey = active.IsBuiltIn ? _relaySettings.ApiKey : active.ApiKey
            };
        }
    }

    public async Task<string> GetActiveHubId()
    {
        await EnsureLoadedAsync();
        lock (_gate)
        {
            return _activeHubId;
        }
    }

    public async Task<IReadOnlyList<HubConfigData>> GetHubs()
    {
        await EnsureLoadedAsync();
        lock (_gate)
        {
            return _hubs.Values
                .OrderByDescending(h => h.IsBuiltIn)
                .ThenBy(h => h.Name, StringComparer.Ordinal)
                .ToList();
        }
    }

    public async Task AddHub(HubConfigData hub)
    {
        await _operationGate.WaitAsync();
        try
        {
            if (string.IsNullOrWhiteSpace(hub.Id))
            {
                throw new ArgumentException("Hub id is required.", nameof(hub));
            }

            if (hub.Id == BuiltInHubId)
            {
                throw new ArgumentException($"Hub '{hub.Id}' is reserved.", nameof(hub));
            }

            await EnsureLoadedAsync();
            lock (_gate)
            {
                if (_hubs.ContainsKey(hub.Id))
                {
                    throw new ArgumentException($"Hub '{hub.Id}' already exists.", nameof(hub));
                }

                _hubs[hub.Id] = hub with { IsBuiltIn = false };
            }

            try
            {
                await PersistAsync();
            }
            catch
            {
                lock (_gate)
                {
                    _hubs.Remove(hub.Id);
                }

                throw;
            }
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public async Task UpdateHub(string id, string name, string baseUrl, string apiKey)
    {
        await _operationGate.WaitAsync();
        try
        {
            await EnsureLoadedAsync();
            HubConfigData previous;
            lock (_gate)
            {
                if (!_hubs.TryGetValue(id, out var existing))
                {
                    throw new ArgumentException($"Hub '{id}' does not exist.", nameof(id));
                }

                if (existing.IsBuiltIn)
                {
                    throw new InvalidOperationException("The built-in hub cannot be edited.");
                }

                previous = existing;
                _hubs[id] = existing with { Name = name, BaseUrl = baseUrl, ApiKey = apiKey };
            }

            try
            {
                await PersistAsync();
            }
            catch
            {
                lock (_gate)
                {
                    _hubs[id] = previous;
                }

                throw;
            }
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public async Task RemoveHub(string id)
    {
        await _operationGate.WaitAsync();
        try
        {
            await EnsureLoadedAsync();
            HubConfigData? previous;
            var wasActive = false;
            lock (_gate)
            {
                if (!_hubs.TryGetValue(id, out var existing))
                {
                    throw new ArgumentException($"Hub '{id}' does not exist.", nameof(id));
                }

                if (existing.IsBuiltIn)
                {
                    throw new InvalidOperationException("The built-in hub cannot be removed.");
                }

                previous = existing;
                wasActive = _activeHubId == id;
                _hubs.Remove(id);
                if (wasActive)
                {
                    _activeHubId = BuiltInHubId;
                }
            }

            try
            {
                await PersistAsync();
            }
            catch
            {
                lock (_gate)
                {
                    _hubs[id] = previous;
                    if (wasActive)
                    {
                        _activeHubId = id;
                    }
                }

                throw;
            }
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public async Task SelectHub(string id)
    {
        await _operationGate.WaitAsync();
        try
        {
            await EnsureLoadedAsync();
            string previousActiveHubId;
            lock (_gate)
            {
                if (!_hubs.ContainsKey(id))
                {
                    throw new ArgumentException($"Hub '{id}' does not exist.", nameof(id));
                }

                previousActiveHubId = _activeHubId;
                _activeHubId = id;
            }

            try
            {
                await PersistAsync();
            }
            catch
            {
                lock (_gate)
                {
                    _activeHubId = previousActiveHubId;
                }

                throw;
            }
        }
        finally
        {
            _operationGate.Release();
        }
    }

    private Task EnsureLoadedAsync() => _loadTask;

    private async Task LoadAsync()
    {
        try
        {
            var json = await _storage.LoadValueAsync(StorageKey);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            var state = JsonSerializer.Deserialize<HubConfigurationsState>(json);
            if (state?.Hubs is null)
            {
                return;
            }

            lock (_gate)
            {
                var knownIds = new HashSet<string>(StringComparer.Ordinal) { BuiltInHubId };
                foreach (var hub in state.Hubs)
                {
                    if (string.IsNullOrWhiteSpace(hub.Id) || !knownIds.Add(hub.Id))
                    {
                        continue;
                    }

                    _hubs[hub.Id] = hub with { IsBuiltIn = false };
                }

                if (!string.IsNullOrWhiteSpace(state.ActiveHubId) && _hubs.ContainsKey(state.ActiveHubId))
                {
                    _activeHubId = state.ActiveHubId;
                }
            }
        }
        catch
        {
            // Missing or malformed data keeps the built-in hub selection.
        }
    }

    private async Task PersistAsync()
    {
        HubConfigurationsState state;
        lock (_gate)
        {
            state = new HubConfigurationsState
            {
                Hubs = _hubs.Values.Where(h => !h.IsBuiltIn).ToList(),
                ActiveHubId = _activeHubId
            };
        }

        await _storage.SaveValueAsync(StorageKey, JsonSerializer.Serialize(state));
    }

    private sealed class HubConfigurationsState
    {
        public List<HubConfigData> Hubs { get; init; } = [];
        public string? ActiveHubId { get; init; }
    }
}
