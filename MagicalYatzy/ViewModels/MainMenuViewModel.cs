using Sanet.MagicalYatzy.ViewModels.Base;
using System;
using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Resources;
using System.Windows.Input;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Services.Navigation;

namespace Sanet.MagicalYatzy.ViewModels;

public class MainMenuViewModel : DicePanelViewModel
{
    private readonly IPlayerService _playerService;
    private readonly IExternalNavigationService _externalNavigationService;

    public MainMenuViewModel(IDicePanel dicePanel,
        IExternalNavigationService externalNavigationService,
        IPlayerService playerService) : base(dicePanel)
    {
        _externalNavigationService = externalNavigationService;
        _playerService = playerService;

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
    #endregion

    #region Methods
    public void FillMainActions()
    {
        MenuActions = new List<MainMenuAction>
        {
            new MainMenuAction
            {
                Label = Strings.NewOnlineGameAction,
                MenuAction = new SimpleCommand(() => { /*_navigationService.NavigateToPage(AppPages.OnlineLobbyPage);*/ }),
                Description = Strings.NewOnlineGameDescription,
                Image = "OnlineGame.png",
            },
            new MainMenuAction
            {
                Label = Strings.NewLocalGameAction,
                MenuAction = new SimpleCommand(() => { NavigationService.NavigateToViewModelAsync<LobbyViewModel>(); }),
                Description = Strings.NewLocalGameDescription,
                Image = "SanetDice.png",
            },
            new MainMenuAction
            {
                Label = Strings.SettingsAction,
                MenuAction = new SimpleCommand(() => { /*_navigationService.NavigateToPage(AppPages.SettingsPage);*/ }),
                Description = Strings.SettingsDescription,
                Image = "Settings.png",
            },

            new MainMenuAction
            {
                Label = Strings.LeaderboardAction,
                MenuAction = new SimpleCommand(() => { /*_navigationService.NavigateToPage(AppPages.LeaderboardPage);*/ }),
                Description = Strings.LeaderboardDescription,
                Image = "Victory.png",
            },
            new MainMenuAction
            {
                Label = Strings.AboutAction,
                MenuAction = new SimpleCommand(() => { /*_navigationService.NavigateToPage(AppPages.AboutPage);*/ }),
                Description = Strings.AboutDescription,
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
                Label = Strings.SendFeedbackAction,
                MenuAction = new SimpleCommand(_externalNavigationService.SendFeedback),
                Image = "Mail.png"
            },
            new MainMenuAction
            {
                Label = Strings.ReviewAppAction,
                MenuAction = new SimpleCommand(_externalNavigationService.RateApp),
                Image = "Rate.png"
            },
            new MainMenuAction
            {
                Label = Strings.FBPage,
                MenuAction = new SimpleCommand(_externalNavigationService.OpenYatzyFBPage),
                Image = "Facebook.png"
            },
            new MainMenuAction
            {
                Label = Strings.ShareApp,
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
        await LoadLocalPlayersAsync();
    }

    public override void DetachHandlers()
    {
        _playerService.PlayersUpdated -= OnPlayersUpdated;
    }

    void OnPlayersUpdated(object sender, EventArgs e)
    {
        NotifyPropertyChanged(nameof(PlayerName));
        NotifyPropertyChanged(nameof(PlayerImage));
    }
}