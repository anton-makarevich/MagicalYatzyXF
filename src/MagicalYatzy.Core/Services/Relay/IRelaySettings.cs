namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// User-editable relay connection settings.
/// </summary>
public interface IRelaySettings
{
    string BaseUrl { get; set; }
    string ApiKey { get; set; }
}