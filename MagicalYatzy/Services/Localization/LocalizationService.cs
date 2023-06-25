using System.Globalization;
using System.Reflection;
using System.Resources;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Resources;

namespace Sanet.MagicalYatzy.Services.Localization;

public class LocalizationService : ILocalizationService
{
    private const string ResourcePath = "Sanet.MagicalYatzy.Resources.Strings";
    private const string KeyIsNull = "KeyIsNull";

    private readonly ResourceManager _resourceManager;

    public LocalizationService()
    {
        SetSystemCulture(LanguageCode.Default);

        _resourceManager = new ResourceManager(ResourcePath, GetType().GetTypeInfo().Assembly);
    }

    public LanguageCode Language { get; private set; } = LanguageCode.Default;

    private CultureInfo _systemCulture;
    public CultureInfo SystemCulture
    {
        get => _systemCulture;
        private set
        {
            _systemCulture = value;
            Strings.Culture = _systemCulture;
        }
    }

    public void SetSystemCulture(LanguageCode language)
    {
        SystemCulture = language.ToCultureInfo();
        Language = language;
    }

    public void SetSystemCulture(CultureInfo cultureInfo)
    {
        SetSystemCulture(cultureInfo.ToLanguageCode());
    }

    public string GetLocalizedString(string key)
    {
        if (string.IsNullOrEmpty(key))
            return KeyIsNull;

        return _resourceManager.GetString(key, SystemCulture) ?? key;
    }
}