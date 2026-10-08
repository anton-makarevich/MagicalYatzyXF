namespace Sanet.MagicalYatzy.ViewModels;

/// <summary>
/// Result of the "Add Hub" dialog, or null if it was cancelled.
/// </summary>
public record AddHubResult
{
    public string Name { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;
}
