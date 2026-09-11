using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.Services.Navigation;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MVVM.Core.Services;
using Xunit;

namespace MagicalYatzyTests.ViewModels;

public class MainMenuViewModelsTests
{
    private readonly MainMenuViewModel _sut;
        
    private readonly IPlayerService _playerServiceMock = Substitute.For<IPlayerService>();
    private readonly INavigationService _navigationServiceMock = Substitute.For<INavigationService>();
    private readonly IDicePanel _dicePanelMock = Substitute.For<IDicePanel>();
    private readonly ILocalizationService _localizationService;

    public MainMenuViewModelsTests()
    {
        var externalNavigationServiceMock = Substitute.For<IExternalNavigationService>();
        _localizationService = Substitute.For<ILocalizationService>();
        _localizationService.GetLocalizedString("SettingsAction").Returns(Strings.SettingsAction);
        _sut = new MainMenuViewModel(_dicePanelMock, externalNavigationServiceMock, _playerServiceMock, _localizationService);
    }

    [Fact]
    public void FillMainMenuCreatesMainMenu()
    {
        _sut.FillMainActions();

        Assert.True(_sut.MenuActions.Any());
    }

    [Fact]
    public void MainMenuContainsNewLocalGameItem()
    {
        _localizationService.GetLocalizedString("NewLocalGameAction").Returns(Strings.NewLocalGameAction);
        _localizationService.GetLocalizedString("NewLocalGameDescription").Returns(Strings.NewLocalGameDescription);
        _sut.FillMainActions();

        var newLocalGameMenuItem = _sut.MenuActions.FirstOrDefault(mm => mm.Label == Strings.NewLocalGameAction);
        Assert.NotNull(newLocalGameMenuItem);
        Assert.Equal("SanetDice.png", newLocalGameMenuItem.Image);
        Assert.Equal(Strings.NewLocalGameDescription, newLocalGameMenuItem.Description);
    }

    [Fact]
    public void CallingNewLocalGameItemTriggersCorrespondingNavigationServiceMethod()
    {
        _localizationService.GetLocalizedString("NewLocalGameAction").Returns(Strings.NewLocalGameAction);
        _sut.SetNavigationService(_navigationServiceMock);
        _sut.FillMainActions();

        var newLocalGameMenuItem = _sut.MenuActions.FirstOrDefault(mm => mm.Label == Strings.NewLocalGameAction);
        newLocalGameMenuItem?.MenuAction?.Execute(null);
        _navigationServiceMock.Received().NavigateToViewModelAsync<LobbyViewModel>();
    }

    [Fact]
    public void FillSecondaryMenuCreatesSecondaryMenu()
    {
        _sut.FillSecondaryActions();

        Assert.True(_sut.SecondaryMenuActions.Any());
    }

    [Fact]
    public async Task LoadPlayersMethodCallsCorrespondingServiceMethod()
    {
        await _sut.LoadLocalPlayersAsync();

        await _playerServiceMock.Received().LoadPlayersAsync();
    }

    [Fact]
    public async Task OpeningViewModelLoadsPlayers()
    {
        _sut.AttachHandlers();

        await _playerServiceMock.Received().LoadPlayersAsync();
    }

    [Fact]
    public void PlayerNameAndIconComesFromPlayerService()
    {
        const string testName = "SomeTestName";
        const string testImage = "TestImage.jpg";
            
        _playerServiceMock.CurrentPlayer.Name = testName;
        _playerServiceMock.CurrentPlayer.ProfileImage = testImage;
            
        Assert.Equal(testName, _sut.PlayerName);
        Assert.Equal(testImage, _sut.PlayerImage);
    }

    [Fact]
    public void SubscribesToPlayerServiceEventsWhenAttachHandlersIsCalled()
    {
        // Arrange
        var propertyChangedCalledTimes = 0;
        _sut.PropertyChanged+= (s, e) => { propertyChangedCalledTimes++;};
        _sut.AttachHandlers();
            
        // Act
        _playerServiceMock.PlayersUpdated += Raise.Event();
            
        // Assert
        Assert.Equal(2,propertyChangedCalledTimes);
    }
        
    [Fact]
    public void UnsubscribesToPlayerServiceEventsWhenDetachHandlersIsCalled()
    {
        // Arrange
        var propertyChangedCalledTimes = 0;
        _sut.PropertyChanged+= (s, e) => { propertyChangedCalledTimes++;};
        _sut.AttachHandlers();
        _sut.DetachHandlers();
        // Act
        _playerServiceMock.PlayersUpdated += Raise.Event();
            
        // Assert
        Assert.Equal(0,propertyChangedCalledTimes);
    }

    [Fact]
    public void SelectingPlayerInvokesNavigationToLoginPage()
    {
        //Arrange
        _sut.SetNavigationService(_navigationServiceMock);
            
        // Act
        _sut.SelectPlayerCommand.Execute(null);

        // Assert
        _navigationServiceMock.ReceivedWithAnyArgs().ShowViewModelAsync<LoginViewModel>();
    }

    [Fact]
    public async Task SettingSelectedMenuActionInvokesNavigationAction()
    {
        //Arrange
        _sut.SetNavigationService(_navigationServiceMock);
            
        var newLocalGameMenuItem = _sut.MenuActions.FirstOrDefault(mm => mm.Label == Strings.SettingsAction);
        
        // Act
        _sut.SelectedMenuAction = newLocalGameMenuItem;
        
        // Assert
        Assert.Equal(newLocalGameMenuItem, _sut.SelectedMenuAction);
        await _navigationServiceMock.ReceivedWithAnyArgs().NavigateToViewModelAsync<SettingsViewModel>();
    }
}