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
        private MainMenuViewModel _sut;
        private readonly IExternalNavigationService _externalNavigationServiceMock;

        public MainMenuViewModelsTests()
        {
            _externalNavigationServiceMock = Substitute.For<IExternalNavigationService>();
            _sut = new MainMenuViewModel(dicePanelMock, _externalNavigationServiceMock, playerServiceMock);
        }

        [Fact]
        public void FillMainMenuShouldCreateMainMenu()
        {
            _sut.FillMainActions();

            Assert.True(_sut.MenuActions.Any());
        }

        [Fact]
        public void MainMenuShouldContainNewLoacalGameitem()
        {
            _sut.FillMainActions();

            var newLocalGameMenuItem = _sut.MenuActions.FirstOrDefault(mm => mm.Label == Strings.NewLocalGameAction);
            Assert.NotNull(newLocalGameMenuItem);
            Assert.Equal("SanetDice.png", newLocalGameMenuItem.Image);
            Assert.Equal(Strings.NewLocalGameDescription, newLocalGameMenuItem.Description);
        }

        [Fact]
        public void CallingNewLoacalGameitemShouldTriggercorrespondingNavigationServiceMethod()
        {
            _sut.SetNavigationService(navigationServiceMock);
            _sut.FillMainActions();

            var newLocalGameMenuItem = _sut.MenuActions.FirstOrDefault(mm => mm.Label == Strings.NewLocalGameAction);
            newLocalGameMenuItem.MenuAction.Execute(null);
            navigationServiceMock.Received().NavigateToViewModelAsync<LobbyViewModel>();
        }

        [Fact]
        public void FillSecondaryMenuShouldCreateSecondaryMenu()
        {
            _sut.FillSecondaryActions();

            Assert.True(_sut.SecondaryMenuActions.Any());
        }

        [Fact]
        public async Task LoadPlayersMethodShouldCallCorrespondingServiceMethod()
        {
            await _sut.LoadLocalPlayersAsync();

            await playerServiceMock.Received().LoadPlayersAsync();
        }

        [Fact]
        public async Task OpeningViewModelShouldLoadPlayers()
        {
            _sut.AttachHandlers();

            await playerServiceMock.Received().LoadPlayersAsync();
        }
    }
}
