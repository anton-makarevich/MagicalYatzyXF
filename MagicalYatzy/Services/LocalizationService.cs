using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Sanet.MagicalYatzy.Services
{
    public enum LanguageCode { Default, EnUs, RuRu, };

    public class LocalizationService : ILocalizationService
    {
        public LocalizationService()
        {
            SetSystemCulture(GetCurrentCultureInfo());
        }

        public LanguageCode GetCurrentCultureInfo()
        {
            return LanguageCode.Default;
        }

        public LanguageCode Language { get; private set; } = LanguageCode.Default;

        private CultureInfo _systemCulture;
        public CultureInfo SystemCulture
        {
            get { return _systemCulture; }
            private set
            {
                _systemCulture = value;
                Strings.Culture = _systemCulture;
            }
        }

        public void SetSystemCulture(LanguageCode language)
        {
            SystemCulture = language.ToCaltureInfo();
            Language = language;
        }

        public void SetSystemCulture(CultureInfo cultureInfo)
        {
            SetSystemCulture(cultureInfo.ToLanguageCode());
        }
    }
}
