using System;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Sanet.MagicalYatzy.Avalonia.Converters;

public class ImageNameConverter : IValueConverter
{
    public object? Convert(
        object? value, Type targetType, 
        object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value is not string assetPath) return null;
        const string assetsDirectory = "avares://MagicalYatzy.Avalonia/Assets/"; 
        var imagePath = Path.Combine(assetsDirectory, assetPath);

        var asset = AssetLoader.Open(new Uri(imagePath));
        
        return new Bitmap(asset);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}