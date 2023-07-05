using System;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Sanet.MagicalYatzy.Avalonia.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(
        object? value, Type targetType, 
        object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value is bool and true)
        {
            if (App.Current.Resources.TryGetResource("SanetBlueColor", null, out var resource1))
            {
                if (resource1 is Color color)
                {
                    return color;
                }
            }
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}