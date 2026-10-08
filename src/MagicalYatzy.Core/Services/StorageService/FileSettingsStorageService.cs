using System;
using System.IO;
using System.Threading.Tasks;

namespace Sanet.MagicalYatzy.Services.StorageService;

/// <summary>
/// Stores every settings value in its own file under the application's documents folder.
/// Write errors are propagated so callers can roll back their in-memory state.
/// </summary>
public sealed class FileSettingsStorageService : ISettingsStorageService
{
    private const string FileExtension = ".settings";
    private readonly string _baseFolder;

    public FileSettingsStorageService() : this(null)
    {
    }

    public FileSettingsStorageService(string? baseFolder)
    {
        _baseFolder = baseFolder ?? DefaultFolder;
    }

    private static string DefaultFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MagicalYatzy");

    public async Task<string?> LoadValueAsync(string key)
    {
        var path = GetPath(key);
        if (!File.Exists(path))
            return null;
        return await File.ReadAllTextAsync(path);
    }

    public async Task SaveValueAsync(string key, string value)
    {
        Directory.CreateDirectory(_baseFolder);
        await File.WriteAllTextAsync(GetPath(key), value);
    }

    private string GetPath(string key)
    {
        var safeKey = key;
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            safeKey = safeKey.Replace(invalidChar, '_');
        return Path.Combine(_baseFolder, safeKey + FileExtension);
    }
}
