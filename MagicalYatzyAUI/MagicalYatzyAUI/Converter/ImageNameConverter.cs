using System;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace Sanet.MagicalYatzy.Avalonia.Converter;

public class ImageNameConverter : IValueConverter
{
    public object? Convert(
        object? value, Type targetType, 
        object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value is not string assetPath) return null;
        var assetsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
        var imagePath = Path.Combine(assetsDirectory, assetPath);

        return File.Exists(imagePath) 
            ? new Bitmap(imagePath) 
            : null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}