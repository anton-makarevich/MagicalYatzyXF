using Sanet.MagicalYatzy.Models.Game;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Sanet.MagicalYatzy.Xf.Helpers
{
    public static class DiceLoaderHelper
    {
        private static Dictionary<string, SKBitmap> sources = new Dictionary<string, SKBitmap>();
        private static Assembly assembly = typeof(Die).GetTypeInfo().Assembly;

        public static SKBitmap GetDiceImageByPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            path = $"Sanet.MagicalYatzy.Resources.DiceImages.{path}";

            if (sources.ContainsKey(path))
                return sources[path];

            return LoadAndCacheImage(path);
        }

        public static void PreloadImages()
        {
            var allFiles = assembly.GetManifestResourceNames();
            foreach (var file in allFiles)
            {
                if (file.Contains("Dice"))
                    LoadAndCacheImage(file);
            }
        }

        private static SKBitmap LoadAndCacheImage(string path)
        {
            var image = LoadImage(path);
            if (!sources.ContainsKey(path))
                sources.Add(path, image);
            return image;
        }

        private static SKBitmap LoadImage(string path)
        {
            using (Stream stream = assembly.GetManifestResourceStream(path))
            using (SKManagedStream skStream = new SKManagedStream(stream))
            {
                return SKBitmap.Decode(skStream);
            }
        }
    }
}
