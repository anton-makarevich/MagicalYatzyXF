using System.Globalization;
using Sanet.MagicalYatzy.Models;

namespace Sanet.MagicalYatzy.Extensions
{
    public static class LocalizationExtensions
    {
        public static LanguageCode ToLanguageCode(this CultureInfo cultureInfo)
        {
            return cultureInfo.Name.ToLower()[..2] switch
            {
                "ru" => LanguageCode.RuRu,
                "en" => LanguageCode.EnUs,
                _ => LanguageCode.Default
            };
        }

        public static CultureInfo ToCultureInfo(this LanguageCode code)
        {
            return new CultureInfo(code.ToCultureString());
        }

        public static string ToCultureString(this LanguageCode code)
        {
            return code switch
            {
                LanguageCode.RuRu => "ru-RU",
                _ => "en-US"
            };
        }
    }
}
