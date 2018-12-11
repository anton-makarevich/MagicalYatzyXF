using NUnit.Framework;
using Sanet.MagicalYatzy.ViewModels;
using System.Linq;
using MagicalYatzyTests.ViewModelTests.Base;
using Sanet.MagicalYatzy.Services;
using NSubstitute;

namespace MagicalYatzyTests.ViewModelTests
{
    public class MainMenuViewModelsTests:BaseDicePanelViewModelTests
    {
        private MainMenuViewModel _sut;
        private IExternalNavigationService _externalNavigationServiceMock;

        [SetUp]
        public override void Init()
        {
            base.Init();
            _externalNavigationServiceMock = Substitute.For<IExternalNavigationService>();
            _sut = new MainMenuViewModel(dicePanelMock, _externalNavigationServiceMock, playerServiceMock);
        }

        [Test]
        public void FillMainMenuShouldCreateMainMenu()
        {
            _sut.FillMainActions();

            Assert.IsTrue(_sut.MenuActions.Any());
        }

        [Test]
        public void FillSecondaryMenuShouldCreateSecondaryMenu()
        {
            _sut.FillSecondaryActions();

            Assert.IsTrue(_sut.SecondaryMenuActions.Any());
        }
    }
}
