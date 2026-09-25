using System.Linq;
using System.Reflection;

namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// Build-time initial values for relay settings. These values are embedded in the client assembly
/// and can be extracted; they are defaults, not a secure way to store secrets.
/// </summary>
public static class RelayDefaults
{
    private const string DefaultBaseUrl = "http://localhost:8080";

    public static readonly string BaseUrl = GetMetadataValue("RelayBaseUrl") is { } baseUrl
                                            && !string.IsNullOrWhiteSpace(baseUrl)
        ? baseUrl
        : DefaultBaseUrl;

    public static readonly string ApiKey = GetMetadataValue("RelayApiKey") is { } apiKey
                                           && !string.IsNullOrWhiteSpace(apiKey)
        ? apiKey
        : string.Empty;

    private static string? GetMetadataValue(string key) =>
        typeof(RelayDefaults).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == key)
            ?.Value;
}