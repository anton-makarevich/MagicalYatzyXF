using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using Sanet.MagicalYatzy.Models;

namespace Sanet.MagicalYatzy.Services.Localization;

public class GlobalizationInvariantLocalizationService : ILocalizationService
{
    private Dictionary<string, string> _localizedStrings = new();
    private Dictionary<string, string> _defaultLocalizedStrings = new();

    public Language ActiveLanguage { get; private set; }
    public List<Language> Languages { get; private set; }

    public GlobalizationInvariantLocalizationService()
    {
        Languages = GetAvailableLanguages();
        SetActiveLanguage(Languages.FirstOrDefault(l=>l.IsDefault)); // Set the default language (English) initially
    }

    public void SetActiveLanguage(Language language)
    {
        if (language.IsDefault && _defaultLocalizedStrings.Count == 0)
        {
            _defaultLocalizedStrings = LoadLocalizedStrings(language);
        }
        if (language == ActiveLanguage && _localizedStrings?.Count!=0) return;
        _localizedStrings = LoadLocalizedStrings(language);
        ActiveLanguage = language;
    }

    //[ConstantExpected]
    public void SetActiveLanguage(string languageCode)
    {
        var language = Languages.FirstOrDefault(l => l.Code == languageCode);
        SetActiveLanguage(language);
    }

    public void SetActiveLanguage(System.Globalization.CultureInfo cultureInfo)
    {
        // Not supported in this implementation since it requires CultureInfo
        throw new NotSupportedException("Setting culture using CultureInfo is not supported.");
    }

    public string GetLocalizedString(string key)
    {
        if (_localizedStrings.TryGetValue(key, out var localizedString))
        {
            return localizedString;
        }

        return _defaultLocalizedStrings.TryGetValue(key, out localizedString) ? localizedString : key;
    }

    private static Dictionary<string, string> LoadLocalizedStrings(Language language)
    {
        var resxFileName = language.IsDefault 
            ? "Sanet.MagicalYatzy.Resources.Strings.resources"
            : $"Sanet.MagicalYatzy.Resources.Strings-{language.Code}";

        var assembly = typeof(GlobalizationInvariantLocalizationService).GetTypeInfo().Assembly;

        var resourceNames = assembly.GetManifestResourceNames();
        
        var matchingResourceName = resourceNames.FirstOrDefault(resourceName =>
            resourceName.Contains(resxFileName, StringComparison.OrdinalIgnoreCase));

        if (matchingResourceName == null)
        {
            throw new FileNotFoundException($"Resource file not found for language: {language.Code}");
        }

        using var resourceStream = assembly.GetManifestResourceStream(matchingResourceName);
        if (resourceStream == null)
        {
            throw new MissingManifestResourceException($"Resource not found for language: {language.Code}");
        }
        using var resourceReader = new ResourceReader(resourceStream);
        var localizedStrings = new Dictionary<string, string>();

        foreach (DictionaryEntry entry in resourceReader)
        {
            var key = entry.Key.ToString();
            if (key == null) continue;
            var value = entry.Value?.ToString()??key;
            localizedStrings[key] = value;
        }

        return localizedStrings;
    }

    private List<Language> GetAvailableLanguages()
    {
        var assembly = typeof(GlobalizationInvariantLocalizationService).GetTypeInfo().Assembly;

        var resourceNames = assembly.GetManifestResourceNames()
            .Where(resourceName => resourceName.Contains("Strings", StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (resourceNames.Count == 0)
        {
            throw new ApplicationException("Missing localization resources.");
        }

        var defaultLanguageResource = resourceNames.FirstOrDefault(lrn => lrn.Contains("Strings."));
        if (defaultLanguageResource == null)
        {
            throw new ApplicationException("Missing default language resource.");
        }

        var languages = new List<Language>(){new Language("en", true, "english")};

        if (resourceNames.Count > 1)
        {
            languages.AddRange(resourceNames.Where(l => l.Contains("Strings-"))
                .Select(l =>
                {
                    var languageAttributes = l.Split('.').First(p => p.Contains('-')).Split('-');
                    return new Language(languageAttributes[1], false, languageAttributes[2]);
                }).ToList());
        }

        return  languages;
    }
}