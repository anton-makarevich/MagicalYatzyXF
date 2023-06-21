using System;
using Avalonia.Data.Converters;

namespace Sanet.MagicalYatzy.Avalonia.Converter;

public class StringUppercasedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return value is not string stringValue 
            ? string.Empty 
            : stringValue.ToUpper();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}