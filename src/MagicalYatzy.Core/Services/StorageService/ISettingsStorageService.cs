using System.Threading.Tasks;

namespace Sanet.MagicalYatzy.Services.StorageService;

/// <summary>
/// Key/value storage for persisted application settings. Values are plain text and are not encrypted.
/// </summary>
public interface ISettingsStorageService
{
    /// <summary>
    /// Loads the value stored under the given key, or null when the key does not exist.
    /// </summary>
    Task<string?> LoadValueAsync(string key);

    /// <summary>
    /// Saves the value under the given key. Write errors are propagated to the caller.
    /// </summary>
    Task SaveValueAsync(string key, string value);
}
