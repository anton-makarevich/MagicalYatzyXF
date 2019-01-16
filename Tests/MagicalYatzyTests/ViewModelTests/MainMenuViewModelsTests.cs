using Sanet.MagicalYatzy.ViewModels;
using System.Linq;
using MagicalYatzyTests.ViewModelTests.Base;
using Sanet.MagicalYatzy.Services;
using NSubstitute;
using Xunit;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Resources;

namespace MagicalYatzyTests.ViewModelTests
{
    public class MainMenuViewModelsTests:BaseDicePanelViewModelTests
    {
        private readonly MainMenuViewModel _sut;
        private readonly IExternalNavigationService _externalNavigationServiceMock;

        public MainMenuViewModelsTests()
        {
            _externalNavigationServiceMock = Substitute.For<IExternalNavigationService>();
            _sut = new MainMenuViewModel(dicePanelMock, _externalNavigationServiceMock, playerServiceMock);
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
            _sut.FillMainActions();

            var newLocalGameMenuItem = _sut.MenuActions.FirstOrDefault(mm => mm.Label == Strings.NewLocalGameAction);
            Assert.NotNull(newLocalGameMenuItem);
            Assert.Equal("SanetDice.png", newLocalGameMenuItem.Image);
            Assert.Equal(Strings.NewLocalGameDescription, newLocalGameMenuItem.Description);
        }

        [Fact]
        public void CallingNewLocalGameItemTriggersCorrespondingNavigationServiceMethod()
        {
            _sut.SetNavigationService(navigationServiceMock);
            _sut.FillMainActions();

            var newLocalGameMenuItem = _sut.MenuActions.FirstOrDefault(mm => mm.Label == Strings.NewLocalGameAction);
            newLocalGameMenuItem?.MenuAction?.Execute(null);
            navigationServiceMock.Received().NavigateToViewModelAsync<LobbyViewModel>();
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

            await playerServiceMock.Received().LoadPlayersAsync();
        }

        [Fact]
        public async Task OpeningViewModelLoadsPlayers()
        {
            _sut.AttachHandlers();

            await playerServiceMock.Received().LoadPlayersAsync();
        }

        [Fact]
        public void PlayerNameAndIconComesFromPlayerService()
        {
            const string testName = "SomeTestName";
            const string testImage = "TestImage.jpg";
            
            playerServiceMock.CurrentPlayer.Name = testName;
            playerServiceMock.CurrentPlayer.ProfileImage = testImage;
            
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
            playerServiceMock.PlayersUpdated += Raise.Event();
            
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
            playerServiceMock.PlayersUpdated += Raise.Event();
            
            // Assert
            Assert.Equal(0,propertyChangedCalledTimes);
        }

        [Fact]
        public void SelectingPlayerInvokesNavigationToLoginPage()
        {
            //Arrange
            _sut.SetNavigationService(navigationServiceMock);
            
            // Act
            _sut.SelectPlayerCommand.Execute(null);

            // Assert
            navigationServiceMock.ReceivedWithAnyArgs().ShowViewModelAsync<LoginViewModel>();
        }
    }
}
