using Sanet.MagicalYatzy.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Sanet.MagicalYatzy.Extensions
{
    public static class LocalizationExtensions
    {
        public static LanguageCode ToLanguageCode(this CultureInfo info)
        {
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
