using System.Globalization;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Resources;

namespace Sanet.MagicalYatzy.Services
{
    public interface ILocalizationService
    {
        LanguageCode Language { get; }
        CultureInfo SystemCulture { get; }

        void SetSystemCulture(LanguageCode language);
        void SetSystemCulture(CultureInfo cultureInfo);

        string GetLocalizedString(string key);
    }
}
