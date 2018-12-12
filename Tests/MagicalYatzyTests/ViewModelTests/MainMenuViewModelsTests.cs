using Sanet.MagicalYatzy.ViewModels;
using System.Linq;
using MagicalYatzyTests.ViewModelTests.Base;
using Sanet.MagicalYatzy.Services;
using NSubstitute;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests
{
    public class MainMenuViewModelsTests:BaseDicePanelViewModelTests
    {
        private MainMenuViewModel _sut;
        private IExternalNavigationService _externalNavigationServiceMock;

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
        public void FillSecondaryMenuShouldCreateSecondaryMenu()
        {
            _sut.FillSecondaryActions();

            Assert.True(_sut.SecondaryMenuActions.Any());
        }
    }
}
