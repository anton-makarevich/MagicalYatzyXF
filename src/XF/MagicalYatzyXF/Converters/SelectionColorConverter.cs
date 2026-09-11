using System;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Converters
{
    public class SelectionColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is bool boolValue && boolValue) ? Color.FromRgba(1,1,1,0.8) : Color.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}