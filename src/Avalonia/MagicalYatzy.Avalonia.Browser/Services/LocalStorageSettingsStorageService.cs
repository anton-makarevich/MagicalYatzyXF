using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Services.StorageService;

namespace Sanet.MagicalYatzy.Avalonia.Browser.Services;

/// <summary>
/// Keeps settings values in the browser's localStorage so they survive page reloads.
/// Keys are prefixed to avoid collisions with other applications on the same origin.
/// </summary>
public sealed partial class LocalStorageSettingsStorageService : ISettingsStorageService
{
    private const string KeyPrefix = "MagicalYatzy_";

    [JSImport("globalThis.localStorage.getItem")]
    private static partial string? GetItem(string key);

    [JSImport("globalThis.localStorage.setItem")]
    private static partial void SetItem(string key, string value);

    public Task<string?> LoadValueAsync(string key) => Task.FromResult(GetItem(KeyPrefix + key));

    public Task SaveValueAsync(string key, string value)
    {
        SetItem(KeyPrefix + key, value);
        return Task.CompletedTask;
    }
}
