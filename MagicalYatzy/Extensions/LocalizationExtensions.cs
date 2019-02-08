using System.Globalization;
using Sanet.MagicalYatzy.Models;

namespace Sanet.MagicalYatzy.Extensions
{
    public static class LocalizationExtensions
    {
        public static LanguageCode ToLanguageCode(this CultureInfo cultureInfo)
        {
            switch (cultureInfo.Name.ToLower())
            {
                case "ru-ru":
                    return LanguageCode.RuRu;
            }
            return LanguageCode.Default;
        }

        public static CultureInfo ToCaltureInfo(this LanguageCode code)
        {
            return new CultureInfo(code.ToCultureString());
        }

        public static string ToCultureString(this LanguageCode code)
        {
            switch (code)
            {
                case LanguageCode.RuRu:
                    return "ru-RU";
            }
            return "en-US";
        }
    }
}
