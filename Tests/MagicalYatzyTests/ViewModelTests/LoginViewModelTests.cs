using System.Threading.Tasks;
using Sanet.MagicalYatzy.ViewModels;
using MagicalYatzyTests.ViewModelTests.Base;
using MagicalYatzyTests.ServiceTests;
using MagicalYatzyTests.ServiceTests.Game;
using NSubstitute;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Navigation;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests
{
    public class LoginViewModelTests
    {
        private readonly IPlayerService _playerServiceMock = Substitute.For<IPlayerService>();
        private readonly INavigationService _navigationServiceMock = Substitute.For<INavigationService>();
        
        private readonly LoginViewModel _sut;

        public LoginViewModelTests()
        {
            _sut = new LoginViewModel(_playerServiceMock);
            _sut.SetNavigationService(_navigationServiceMock);
        }

        [Fact]
        public void SuccessfulLoginCallsBackNavigation()
        {
            _playerServiceMock.LoginAsync(PlayerServiceTests.TestUserName, PlayerServiceTests.TestUserPassword).Returns(Task.FromResult(true));

            _sut.NewUsername = PlayerServiceTests.TestUserName;
            _sut.NewPassword = PlayerServiceTests.TestUserPassword;

            _sut.LoginCommand.Execute(null);

            _navigationServiceMock.Received().CloseAsync();
        }

        [Fact] 
        public void CloseCommandShouldCallBackNavigation()
        {
            _sut.CloseCommand.Execute(null);
            _navigationServiceMock.Received().CloseAsync();
        }

        [Fact]
        public void CloseImageHasCorrectValue()
        {
            Assert.Equal("close.png", _sut.CloseImage);
        }
    }
}
