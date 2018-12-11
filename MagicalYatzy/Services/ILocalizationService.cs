using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Sanet.MagicalYatzy.Services
{
    public interface ILocalizationService
    {
        LanguageCode Language { get; }

        CultureInfo SystemCulture { get; }
        void SetSystemCulture(LanguageCode language);
        LanguageCode GetCurrentCultureInfo();
    }
}
