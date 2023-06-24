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
    private const double DefaultPanelWidth = 600;
    public object? Convert(
        object? value, Type targetType, 
        object? parameter,
        System.Globalization.CultureInfo culture)
    {
        return value is not Size size 
            ? DefaultPanelWidth 
            : Math.Min(size.Width, DefaultPanelWidth);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}