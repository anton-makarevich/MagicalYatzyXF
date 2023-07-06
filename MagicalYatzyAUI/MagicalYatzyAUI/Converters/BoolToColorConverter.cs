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
        if (value is true && parameter is string parameterValue)
        {
            if (App.Current.Resources.TryGetResource(parameterValue, null, out var resource1))
            {
                if (resource1 is Color color)
                {
                    return new SolidColorBrush(color);
                }
            }
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}