using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models;

namespace Sanet.MagicalYatzy.Services.Localization;

public class GlobalizationInvariantLocalizationService : ILocalizationService
{
    private Dictionary<string, string> _localizedStrings = new();
    private Dictionary<string, string> _defaultLocalizedStrings = new();

    public LanguageCode Language { get; private set; }

    public GlobalizationInvariantLocalizationService()
    {
        SetSystemCulture(LanguageCode.Default); // Set the default language (English) initially
    }

    public void SetSystemCulture(LanguageCode language)
    {
        if (language == LanguageCode.Default && _defaultLocalizedStrings.Count == 0)
        {
            _defaultLocalizedStrings = LoadLocalizedStrings(language);
        }
        if (language == Language && _localizedStrings?.Count!=0) return;
        _localizedStrings = LoadLocalizedStrings(language);
        Language = language;
    }

    public void SetSystemCulture(System.Globalization.CultureInfo cultureInfo)
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

    private static Dictionary<string, string> LoadLocalizedStrings(LanguageCode language)
    {
        var languageCode = language.ToCultureString();
        var resxFileName = language == LanguageCode.Default 
            ? "Sanet.MagicalYatzy.Resources.Strings.resources"
            : $"Sanet.MagicalYatzy.Resources.Strings-{languageCode}.resources";

        var assembly = typeof(GlobalizationInvariantLocalizationService).GetTypeInfo().Assembly;

        var resourceNames = assembly.GetManifestResourceNames();
        var matchingResourceName = resourceNames.FirstOrDefault(resourceName =>
            resourceName.EndsWith(resxFileName, StringComparison.OrdinalIgnoreCase));

        if (matchingResourceName == null)
        {
            throw new FileNotFoundException($"Resource file not found for language: {languageCode}");
        }

        using var resourceStream = assembly.GetManifestResourceStream(matchingResourceName);
        if (resourceStream == null)
        {
            throw new MissingManifestResourceException($"Resource not found for language: {languageCode}");
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
}