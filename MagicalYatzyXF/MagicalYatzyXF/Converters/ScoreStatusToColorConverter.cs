using System;
using Sanet.MagicalYatzy.Models.Game;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Converters
{
    public class ScoreStatusToColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is ScoreStatus status) || status == ScoreStatus.NoValue)
            {
                return Color.Transparent;
            }

            return (status == ScoreStatus.Bonus)? Color.Gold : (Color)Application.Current.Resources["SanetBlueColor"];
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}