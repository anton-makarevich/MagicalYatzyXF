namespace Sanet.MagicalYatzy.Services.Relay;

/// <summary>
/// Build-time initial values for relay settings. The values are compiled into the client assembly
/// and can be extracted; they are defaults, not a secure way to store secrets. They arrive via the
/// RelayBaseUrl/RelayApiKey MSBuild properties, emitted as constants in a generated partial
/// (see RelayDefaults.targets) — no reflection.
/// </summary>
public static partial class RelayDefaults
{
    private const string DefaultBaseUrl = "http://localhost:8080";

    public static string BaseUrl => BuildTimeBaseUrl ?? DefaultBaseUrl;

    public static string ApiKey => BuildTimeApiKey ?? string.Empty;
}
