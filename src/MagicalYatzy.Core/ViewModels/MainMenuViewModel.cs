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
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.ViewModels;

public class MainMenuViewModel : DicePanelViewModel
{
    private readonly IPlayerService _playerService;
    private readonly ILocalizationService _localizationService;
    private readonly IExternalNavigationService _externalNavigationService;
    
    private MainMenuActionViewModel _selectedMenuAction;

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

    public List<MainMenuActionViewModel> MenuActions { get; private set; }

    public List<MainMenuActionViewModel> SecondaryMenuActions { get; private set; }

    #endregion

    #region Commands
    public ICommand SelectPlayerCommand => new SimpleCommand(() =>
    {
        NavigationService.ShowViewModelAsync<LoginViewModel>();

        NotifyPropertyChanged(nameof(PlayerName));
        NotifyPropertyChanged(nameof(PlayerImage));
    });

    public MainMenuActionViewModel SelectedMenuAction
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
        NotifyPropertyChanged(nameof(MenuActions));
        MenuActions = new List<MainMenuActionViewModel>
        {
            new(new MainMenuAction(
                    "NewOnlineGameAction",
                    "NewOnlineGameDescription",
                    "OnlineGame.png",
                    new SimpleCommand(() =>
                    {
                        /*_navigationService.NavigateToPage(AppPages.OnlineLobbyPage);*/
                    })),
                _localizationService),
            new(new MainMenuAction(
                    "NewLocalGameAction",
                    "NewLocalGameDescription",
                    "SanetDice.png",
                    new AsyncCommand(
                        async () => { await NavigationService.NavigateToViewModelAsync<LobbyViewModel>(); })),
                _localizationService),
            new(new MainMenuAction(
                    "SettingsAction",
                    "SettingsDescription",
                    "Settings.png",
                    new AsyncCommand(async () =>
                    {
                        await NavigationService.NavigateToViewModelAsync<SettingsViewModel>();
                    })),
                _localizationService),
            new MainMenuActionViewModel(new MainMenuAction(
                    "LeaderboardAction",
                    "LeaderboardDescription",
                    "Victory.png",
                    new SimpleCommand(() =>
                    {
                        /*_navigationService.NavigateToPage(AppPages.LeaderboardPage);*/
                    })),
                _localizationService),
            new(new MainMenuAction(
                    "AboutAction",
                    "AboutDescription",
                    "About.png",
                    new SimpleCommand(() =>
                    {
                        /*_navigationService.NavigateToPage(AppPages.AboutPage);*/
                    })),
                _localizationService)
        };
    }
    public void FillSecondaryActions()
    {
        SecondaryMenuActions = new List<MainMenuActionViewModel>
        {
            new(new MainMenuAction(
                    "SendFeedbackAction",
                    "",
                    "Mail.png",
                    new SimpleCommand(_externalNavigationService.SendFeedback)),
                _localizationService),
            new(new MainMenuAction(
                    "ReviewAppAction",
                    "",
                    "Rate.png",
                    new SimpleCommand(_externalNavigationService.RateApp)),
                _localizationService),
            new(new MainMenuAction(
                    "FBPage",
                    "",
                    "Facebook.png",
                    new SimpleCommand(_externalNavigationService.OpenYatzyFBPage)),
                _localizationService),
            new(new MainMenuAction(
                    "ShareApp",
                    "",
                    "Share.png",
                    new SimpleCommand(() => { })),
                _localizationService)
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