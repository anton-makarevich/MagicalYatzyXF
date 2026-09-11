using System;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Avalonia.Converters;

public class ScoreToColorConverter: IValueConverter
{
    public object? Convert(
        object? value, Type targetType, 
        object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value is Scores score)
        {
            var resourceKey = score switch
            {
                Scores.FullHouse 
                    or Scores.FourOfAKind 
                    or Scores.ThreeOfAKind 
                    or Scores.SmallStraight 
                    or Scores.LargeStraight => "SanetBlueColor",
                Scores.Kniffel => "GoldBackColor",
                Scores.Chance 
                    or Scores.Bonus =>"BronzeBackColor",
                _ => "SilverBackColor"
            };
            if (App.Current.Resources.TryGetResource(resourceKey, null, out var resource1))
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