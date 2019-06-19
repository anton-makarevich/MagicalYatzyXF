using System;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Converters
{
    public class StringUppercasedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var stringValue = value as string;
            if (stringValue == null)
            {
                return string.Empty;
            }

            return stringValue.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
