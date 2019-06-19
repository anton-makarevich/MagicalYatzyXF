using Sanet.MagicalYatzy.Xf.Helpers;
using System;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Converters
{
    public class ImageNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string stringValue))
            {
                return string.Empty;
            }

            return ImageHelper.Get(stringValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
