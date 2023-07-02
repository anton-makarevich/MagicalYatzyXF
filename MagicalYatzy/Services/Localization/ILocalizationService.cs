using System.Globalization;
using Sanet.MagicalYatzy.Models;

namespace Sanet.MagicalYatzy.Services.Localization;

public interface ILocalizationService
{
    LanguageCode Language { get; }

    void SetSystemCulture(LanguageCode language);
    void SetSystemCulture(CultureInfo cultureInfo);

    string GetLocalizedString(string key);
}