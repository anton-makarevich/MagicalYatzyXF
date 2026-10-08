using System;

namespace Sanet.MagicalYatzy.ViewModels;

/// <summary>
/// Shared validation rule for relay hub base URLs, used by both the add-hub dialog
/// and the settings row editor.
/// </summary>
public static class HubUrlValidator
{
    public static bool IsValid(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return false;
        return Uri.TryCreate(baseUrl.Trim(), UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
