using System;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Converters
{
    public class BoolToSanetColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is bool boolValue && boolValue)
                ? (Color) Application.Current.Resources["SanetBlueColor"]
                : Color.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}