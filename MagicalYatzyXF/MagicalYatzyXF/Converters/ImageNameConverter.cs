using Sanet.MagicalYatzy.XF.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Converters
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
