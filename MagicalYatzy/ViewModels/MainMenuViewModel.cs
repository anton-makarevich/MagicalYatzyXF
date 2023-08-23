using Sanet.MagicalYatzy.ViewModels.Base;
using System;
using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Services.Game;
using System.Windows.Input;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.Services.Navigation;

namespace Sanet.MagicalYatzy.ViewModels;

public class MainMenuViewModel : DicePanelViewModel
{
    private readonly IPlayerService _playerService;
    private readonly ILocalizationService _localizationService;
    private readonly IExternalNavigationService _externalNavigationService;
    
    private MainMenuAction _selectedMenuAction;

    public MainMenuViewModel(IDicePanel dicePanel,
        IExternalNavigationService externalNavigationService,
        IPlayerService playerService,
        ILocalizationService localizationService) : base(dicePanel)
    {
        _externalNavigationService = externalNavigationService;
        _playerService = playerService;
        _localizationService = localizationService;

        FillMainActions();
        FillSecondaryActions();
    }

    #region Properties

    public string PlayerName => _playerService.CurrentPlayer?.Name;

    public string PlayerImage => _playerService.CurrentPlayer?.ProfileImage;

    public List<MainMenuAction> MenuActions { get; private set; }

    public List<MainMenuAction> SecondaryMenuActions { get; private set; }

    #endregion

    #region Commands
    public ICommand SelectPlayerCommand => new SimpleCommand(() =>
    {
        NavigationService.ShowViewModelAsync<LoginViewModel>();

        NotifyPropertyChanged(nameof(PlayerName));
        NotifyPropertyChanged(nameof(PlayerImage));
    });

    public MainMenuAction SelectedMenuAction
    {
        get => _selectedMenuAction;
        set
        {
            value?.MenuAction.Execute(null);
            SetProperty(ref _selectedMenuAction, value);
        }
    }

    #endregion

    #region Methods
    public void FillMainActions()
    {
        MenuActions = new List<MainMenuAction>
        {
            new MainMenuAction
            {
                Label = _localizationService.GetLocalizedString("NewOnlineGameAction"),
                MenuAction = new SimpleCommand(() => { /*_navigationService.NavigateToPage(AppPages.OnlineLobbyPage);*/ }),
                Description = _localizationService.GetLocalizedString("NewOnlineGameDescription"),
                Image = "OnlineGame.png",
            },
            new MainMenuAction
            {
                Label = _localizationService.GetLocalizedString("NewLocalGameAction"),
                MenuAction = new AsyncCommand(async () => { await NavigationService.NavigateToViewModelAsync<LobbyViewModel>(); }),
                Description = _localizationService.GetLocalizedString("NewLocalGameDescription"),
                Image = "SanetDice.png",
            },
            new MainMenuAction
            {
                Label = _localizationService.GetLocalizedString("SettingsAction"),
                MenuAction = new AsyncCommand(async () => {await NavigationService.NavigateToViewModelAsync<SettingsViewModel>(); }),
                Description = _localizationService.GetLocalizedString("SettingsDescription"),
                Image = "Settings.png",
            },

            new MainMenuAction
            {
                Label = _localizationService.GetLocalizedString("LeaderboardAction"),
                MenuAction = new SimpleCommand(() => { /*_navigationService.NavigateToPage(AppPages.LeaderboardPage);*/ }),
                Description = _localizationService.GetLocalizedString("LeaderboardDescription"),
                Image = "Victory.png",
            },
            new MainMenuAction
            {
                Label = _localizationService.GetLocalizedString("AboutAction"),
                MenuAction = new SimpleCommand(() => { /*_navigationService.NavigateToPage(AppPages.AboutPage);*/ }),
                Description = _localizationService.GetLocalizedString("AboutDescription"),
                Image = "About.png",
            }
        };
        NotifyPropertyChanged(nameof(MenuActions));
    }
    public void FillSecondaryActions()
    {
        SecondaryMenuActions = new List<MainMenuAction>
        {
            new MainMenuAction
            {
                Label =_localizationService.GetLocalizedString("SendFeedbackAction"),
                MenuAction = new SimpleCommand(_externalNavigationService.SendFeedback),
                Image = "Mail.png"
            },
            new MainMenuAction
            {
                Label = _localizationService.GetLocalizedString("ReviewAppAction"),
                MenuAction = new SimpleCommand(_externalNavigationService.RateApp),
                Image = "Rate.png"
            },
            new MainMenuAction
            {
                Label = _localizationService.GetLocalizedString("FBPage"),
                MenuAction = new SimpleCommand(_externalNavigationService.OpenYatzyFBPage),
                Image = "Facebook.png"
            },
            new MainMenuAction
            {
                Label = _localizationService.GetLocalizedString("ShareApp"),
                MenuAction = new SimpleCommand(() =>
                {
                }),
                Image = "Share.png"
            }
        };

        NotifyPropertyChanged(nameof(SecondaryMenuActions));
    }

    internal async Task LoadLocalPlayersAsync()
    {
        await _playerService.LoadPlayersAsync();
    }
    #endregion

    public override async void AttachHandlers()
    {
        _playerService.PlayersUpdated += OnPlayersUpdated;
        SelectedMenuAction = null;
        await LoadLocalPlayersAsync();
    }

    public override void DetachHandlers()
    {
        _playerService.PlayersUpdated -= OnPlayersUpdated;
        SelectedMenuAction = null;
    }

    void OnPlayersUpdated(object sender, EventArgs e)
    {
        NotifyPropertyChanged(nameof(PlayerName));
        NotifyPropertyChanged(nameof(PlayerImage));
    }
}