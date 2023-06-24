using System;
using System.IO;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Sanet.MagicalYatzy.Avalonia.Converter;

public class PanelWidthConverter : IValueConverter
{
    public object? Convert(
        object? value, Type targetType, 
        object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value is not Size size)
        {
            return 600;
        }
        return Math.Min(size.Width, 600);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}