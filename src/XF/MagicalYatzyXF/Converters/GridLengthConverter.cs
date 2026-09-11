using System;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Converters
{
    public class GridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out var doubleValue))
            {
                return new GridLength(doubleValue);
            }

            return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}