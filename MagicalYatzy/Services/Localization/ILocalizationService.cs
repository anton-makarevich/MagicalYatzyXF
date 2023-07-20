using System.Collections.Generic;
using Sanet.MagicalYatzy.Models;

namespace Sanet.MagicalYatzy.Services.Localization;

public interface ILocalizationService
{
    Language ActiveLanguage { get; }
    List<Language> Languages { get; }

    void SetActiveLanguage(Language language);
    
    void SetActiveLanguage(string languageCode);

    string GetLocalizedString(string key);
}