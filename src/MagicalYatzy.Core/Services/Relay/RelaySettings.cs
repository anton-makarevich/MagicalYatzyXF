namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// In-memory relay settings initialized from build-time defaults.
/// </summary>
public sealed class RelaySettings : IRelaySettings
{
    public string BaseUrl { get; set; } = RelayDefaults.BaseUrl;
    public string ApiKey { get; set; } = RelayDefaults.ApiKey;
}