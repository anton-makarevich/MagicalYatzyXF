using System.Threading.Tasks;
using Sanet.MagicalYatzy.ViewModels;
using MagicalYatzyTests.ViewModelTests.Base;
using MagicalYatzyTests.ServiceTests;
using MagicalYatzyTests.ServiceTests.Game;
using NSubstitute;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests
{
    public class LoginViewModelTests:BaseViewModelTests
    {
        private readonly LoginViewModel _sut;

        public LoginViewModelTests()
        {
            _sut = new LoginViewModel(playerServiceMock);
            _sut.SetNavigationService(navigationServiceMock);
        }

        [Fact]
        public void SuccessfulLoginCallsBackNavigation()
        {
            playerServiceMock.LoginAsync(PlayerServiceTests.TestUserName, PlayerServiceTests.TestUserPassword).Returns(Task.FromResult(true));

            _sut.NewUsername = PlayerServiceTests.TestUserName;
            _sut.NewPassword = PlayerServiceTests.TestUserPassword;

            _sut.LoginCommand.Execute(null);

            navigationServiceMock.Received().CloseAsync();
        }

        [Fact] 
        public void CloseCommandShouldCallBackNavigation()
        {
            _sut.CloseCommand.Execute(null);
            navigationServiceMock.Received().CloseAsync();
        }

        [Fact]
        public void CloseImageHasCorrectValue()
        {
            Assert.Equal("close.png", _sut.CloseImage);
        }
    }
}
