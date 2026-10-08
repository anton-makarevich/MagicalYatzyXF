using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Sanet.Localization;
using Sanet.Transport.SignalR.Client.Relay;

namespace Sanet.MagicalYatzy.ViewModels;

public class SettingsViewModel : DicePanelViewModel
{
    private readonly IGameSettingsService _gameSettingsService;
    private readonly ILocalizationService _localizationService;
    private readonly IRelayHubConfigurationProvider _hubConfigurationProvider;
    private readonly IRelayRoomClient _relayRoomClient;
    private HubEntryViewModel? _selectedHub;
    private Task? _selectHubTask;
    private CancellationTokenSource _lifetimeCancellation = new();

    public SettingsViewModel(
        IDicePanel dicePanel,
        IGameSettingsService gameSettingsService,
        ILocalizationService localizationService,
        IRelayHubConfigurationProvider hubConfigurationProvider,
        IRelayRoomClient relayRoomClient):base(dicePanel)
    {
        _gameSettingsService = gameSettingsService;
        _localizationService = localizationService;
        _hubConfigurationProvider = hubConfigurationProvider;
        _relayRoomClient = relayRoomClient;

        AvailableLanguages = new ObservableCollection<Language>(
            _localizationService.Languages);

        AddHubCommand = new AsyncCommand(AddHubAsync);
        RemoveHubCommand = new AsyncCommand<HubEntryViewModel>(RemoveHubAsync);
    }

    #region hub management

    public ICommand AddHubCommand { get; }

    public ICommand RemoveHubCommand { get; }

    public string HubSectionTitle => _localizationService.GetString("HubSectionTitleText");

    public string HubSelectLabel => _localizationService.GetString("SelectHubLabel");

    public string HubAddHubLabel => _localizationService.GetString("AddHubLabel");

    public ObservableCollection<HubEntryViewModel> Hubs { get; } = [];

    public HubEntryViewModel? SelectedHub
    {
        get => _selectedHub;
        set
        {
            if (_selectedHub == value) return;
            _selectedHub = value;
            NotifyPropertyChanged();
            if (value is null) return;
            EnqueueSelect(value.Id);
        }
    }

    private void EnqueueSelect(string id)
    {
        var previous = _selectHubTask;
        var task = SelectHubChainedAsync(previous, id);
        _selectHubTask = task;
        _ = task;
    }

    private async Task SelectHubChainedAsync(Task? previous, string id)
    {
        if (previous != null)
        {
            try
            {
                await previous;
            }
            catch
            {
            }
        }

        try
        {
            await _hubConfigurationProvider.SelectHub(id);
        }
        catch
        {
        }
    }

    #endregion

    #region bind props

    public string Title => _localizationService.GetString("SettingsCaptionText");

    public string SettingsStyleCaption => _localizationService.GetString("SettingsStyleCaptionText");

    public string AngleLowText => _localizationService.GetString("AngLowText");

    public string AngleHighText => _localizationService.GetString("AngHighText");

    public string AngleVeryHighText => _localizationService.GetString("AngVeryHighText");

    public string SettingsAngleCaption => _localizationService.GetString("SettingsAngleCaptionText");

    public string SpeedSlow => _localizationService.GetString("SpeedSlowText");

    public string SpeedVerySlow => _localizationService.GetString("SpeedVerySlowText");

    public string SpeedFast => _localizationService.GetString("SpeedFastText");

    public string SpeedVeryFast => _localizationService.GetString("SpeedVeryFastText");

    public string SettingsSpeedCaption => _localizationService.GetString("SettingsSpeedCaptionText");

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
    public string SoundLabel => _localizationService.GetString("SoundLabel");
    public string OffContent => _localizationService.GetString("OffContent");
    public string OnContent => _localizationService.GetString("OnContent");
    public string LanguageLabel => _localizationService.GetString("LanguageLabel");
    public string BackImage => "Back.png";
    
    public ObservableCollection<Language> AvailableLanguages { get; }

    public Language SelectedLanguage
    {
        get => AvailableLanguages.FirstOrDefault(l => l.Code == _localizationService.ActiveLanguage.Code)
               ?? _localizationService.ActiveLanguage;
        set
        {
            _localizationService.SetActiveLanguage(value);
            NotifyAllPropertiesChanged();
        }
    }

    #endregion

    #region hub workflow

    public override void AttachHandlers()
    {
        base.AttachHandlers();
        _lifetimeCancellation = new CancellationTokenSource();
        _ = LoadHubsAsync();
    }

    public override void DetachHandlers()
    {
        _lifetimeCancellation.Cancel();
        _lifetimeCancellation.Dispose();
        base.DetachHandlers();
    }

    private async Task AddHubAsync()
    {
        var dialog = new AddHubViewModel(_localizationService);
        dialog.SetNavigationService(NavigationService);
        var result = await NavigationService.ShowViewModelForResultAsync<AddHubViewModel, AddHubResult?>(dialog);

        if (result is null) return;

        var hub = new HubConfigData(
            Guid.NewGuid().ToString("N"),
            result.Name,
            result.BaseUrl,
            result.ApiKey,
            false);
        await _hubConfigurationProvider.AddHub(hub);

        await LoadHubsAsync();
    }

    private async Task RemoveHubAsync(HubEntryViewModel? entry)
    {
        if (entry is null || entry.IsBuiltIn) return;

        await _hubConfigurationProvider.RemoveHub(entry.Id);

        await LoadHubsAsync();
    }

    private async Task OnHubSaved(HubEntryViewModel entry)
    {
        var pending = entry.PendingHub;
        await _hubConfigurationProvider.UpdateHub(entry.Id, pending.Name, pending.BaseUrl, pending.ApiKey);

        await LoadHubsAsync();
    }

    private async Task LoadHubsAsync()
    {
        var hubs = await _hubConfigurationProvider.GetHubs();
        var activeHubId = await _hubConfigurationProvider.GetActiveHubId();

        Hubs.Clear();
        foreach (var hub in hubs)
        {
            Hubs.Add(new HubEntryViewModel(
                hub,
                onSaved: OnHubSaved,
                checkStatus: CheckHubStatusAsync,
                localizationService: _localizationService));
        }

        _selectedHub = Hubs.FirstOrDefault(h => h.Id == activeHubId);
        NotifyPropertyChanged(nameof(SelectedHub));

        foreach (var hub in Hubs)
        {
            _ = hub.RefreshStatusAsync(_lifetimeCancellation.Token);
        }
    }

    private async Task<HubStatus> CheckHubStatusAsync(HubEntryViewModel entry, CancellationToken cancellationToken)
    {
        try
        {
            var options = new RelayClientOptions
            {
                BaseUrl = entry.BaseUrl,
                ApiKey = entry.ApiKey
            };
            var error = await _relayRoomClient.Health(cancellationToken, options);
            return error == null ? HubStatus.Online : HubStatus.Offline;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return HubStatus.Offline;
        }
    }

    #endregion
}