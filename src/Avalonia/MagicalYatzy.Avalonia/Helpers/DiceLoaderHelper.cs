using System.Collections.Generic;
using System.Reflection;
using Avalonia.Media.Imaging;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Avalonia.Helpers;

public static class DiceLoaderHelper
{
    private static readonly Dictionary<string, Bitmap> Sources = new();
    private static readonly Assembly Assembly = typeof(Die).GetTypeInfo().Assembly;

    public static Bitmap? GetDiceImageByPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        path = $"Sanet.MagicalYatzy.Resources.DiceImages.{path}";

        return Sources.TryGetValue(path, out var source) ? source : LoadAndCacheImage(path);
    }

    public static void PreloadImages()
    {
        var allFiles = Assembly.GetManifestResourceNames();
        foreach (var file in allFiles)
        {
            if (file.Contains("Dice"))
                LoadAndCacheImage(file);
        }
    }

    private static Bitmap? LoadAndCacheImage(string path)
    {
        var image = LoadImage(path);
        if (image == null) return null;
        Sources.TryAdd(path, image);
        return image;
    }

    private static Bitmap? LoadImage(string path)
    {
        using var stream = Assembly.GetManifestResourceStream(path);
        return stream != null ? new Bitmap(stream) : null;
    }
}