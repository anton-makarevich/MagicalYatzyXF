using System;
using Avalonia;
using Avalonia.Data.Converters;

namespace Sanet.MagicalYatzy.Avalonia.Converters;

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