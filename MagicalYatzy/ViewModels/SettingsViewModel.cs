using System.Collections.ObjectModel;
using System.Linq;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.ViewModels.Base;

namespace Sanet.MagicalYatzy.ViewModels;

public class SettingsViewModel : DicePanelViewModel
{
    private readonly IGameSettingsService _gameSettingsService;
    private readonly ILocalizationService _localizationService;

    public SettingsViewModel(
        IDicePanel dicePanel,
        IGameSettingsService gameSettingsService,
        ILocalizationService localizationService):base(dicePanel)
    {
        _gameSettingsService = gameSettingsService;
        _localizationService = localizationService;

        AvailableLanguages = new ObservableCollection<Language>(
            _localizationService.Languages);
    }     

    #region bind props

    public string Title => _localizationService.GetLocalizedString("SettingsCaptionText");

    public string SettingsStyleCaption => _localizationService.GetLocalizedString("SettingsStyleCaptionText");

    public string AngleLowText => _localizationService.GetLocalizedString("AngLowText");

    public string AngleHighText => _localizationService.GetLocalizedString("AngHighText");

    public string AngleVeryHighText => _localizationService.GetLocalizedString("AngVeryHighText");

    public string SettingsAngleCaption => _localizationService.GetLocalizedString("SettingsAngleCaptionText");

    public string SpeedSlow => _localizationService.GetLocalizedString("SpeedSlowText");

    public string SpeedVerySlow => _localizationService.GetLocalizedString("SpeedVerySlowText");

    public string SpeedFast => _localizationService.GetLocalizedString("SpeedFastText");

    public string SpeedVeryFast => _localizationService.GetLocalizedString("SpeedVeryFastText");

    public string SettingsSpeedCaption => _localizationService.GetLocalizedString("SettingsSpeedCaptionText");

    public int DieAngle
    {
        get => _gameSettingsService.DieAngle;
        set
        {
            if (_gameSettingsService.DieAngle == value) return;
            _gameSettingsService.DieAngle = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged(nameof(IsAngleLow));
            NotifyPropertyChanged(nameof(IsAngleHigh));
            NotifyPropertyChanged(nameof(IsAngleVeryHigh));
        }
    }
    public DiceSpeed DieSpeed
    {
        get => (DiceSpeed)_gameSettingsService.DieSpeed;
        set
        {
            if ((DiceSpeed)_gameSettingsService.DieSpeed == value) return;
            _gameSettingsService.DieSpeed = (int)value;
            NotifyPropertyChanged();
            NotifyPropertyChanged(nameof(IsSpeedVerySlow));
            NotifyPropertyChanged(nameof(IsSpeedSlow));
            NotifyPropertyChanged(nameof(IsSpeedFast));
            NotifyPropertyChanged(nameof(IsSpeedVeryFast));
        }
    }
    public DiceStyle DieStyle
    {
        get => _gameSettingsService.DieStyle;
        set
        {
            if (_gameSettingsService.DieStyle == value) return;
            _gameSettingsService.DieStyle = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged(nameof(IsStyleBlue));
            NotifyPropertyChanged(nameof(IsStyleRed));
            NotifyPropertyChanged(nameof(IsStyleWhite));
        }
    }

    public bool IsStyleBlue
    {
        get => DieStyle == DiceStyle.Blue;
        set
        {
            if (value)
            {
                DieStyle = DiceStyle.Blue;
            }
        }
    }
    
    public bool IsStyleRed
    {
        get => DieStyle == DiceStyle.Red;
        set
        {
            if (value)
            {
                DieStyle = DiceStyle.Red;
            }
        }
    }
    public bool IsStyleWhite
    {
        get => DieStyle == DiceStyle.Classic;
        set
        {
            if (value)
            {
                DieStyle = DiceStyle.Classic;
            }
        }
    }

    public bool IsSpeedVerySlow
    {
        get => DieSpeed== DiceSpeed.VerySlow;
        set
        {
            if (value)
            {
                DieSpeed = DiceSpeed.VerySlow;
            }
        }
    }
    public bool IsSpeedSlow
    {
        get => DieSpeed == DiceSpeed.Slow;
        set
        {
            if (value)
            {
                DieSpeed = DiceSpeed.Slow;
            }
        }
    }
    public bool IsSpeedFast
    {
        get => DieSpeed == DiceSpeed.Fast;
        set
        {
            if (value)
            {
                DieSpeed = DiceSpeed.Fast;
            }
        }
    }
    public bool IsSpeedVeryFast
    {
        get => DieSpeed == DiceSpeed.VeryFast;
        set
        {
            if (value)
            {
                DieSpeed = DiceSpeed.VeryFast;
            }
        }
    }

    public bool IsAngleLow
    {
        get => DieAngle == 0;
        set
        {
            if (value)
            {
                DieAngle = 0;
            }
        }
    }
    public bool IsAngleHigh
    {
        get => DieAngle == 2;
        set
        {
            if (value)
            {
                DieAngle = 2;
            }
        }
    }
    public bool IsAngleVeryHigh
    {
        get => DieAngle == 4;
        set
        {
            if (value)
            {
                DieAngle = 4;
            }
        }
    }

    public bool IsSoundEnabled
    {
        get
        {
            var rv= _gameSettingsService.IsSoundEnabled;
            return rv;
        }
        set
        {
            if (_gameSettingsService.IsSoundEnabled == value) return;
            _gameSettingsService.IsSoundEnabled = value;
            NotifyPropertyChanged();
        }
    }
    public string SoundLabel => _localizationService.GetLocalizedString("SoundLabel");
    public string OffContent => _localizationService.GetLocalizedString("OffContent");
    public string OnContent => _localizationService.GetLocalizedString("OnContent");
    public string LanguageLabel => _localizationService.GetLocalizedString("LanguageLabel");
    public string BackImage => "Back.png";
    
    public ObservableCollection<Language> AvailableLanguages { get; }

    public Language SelectedLanguage
    {
        get => _localizationService.ActiveLanguage;
        set => _localizationService.SetActiveLanguage(value);
    }

    #endregion
}