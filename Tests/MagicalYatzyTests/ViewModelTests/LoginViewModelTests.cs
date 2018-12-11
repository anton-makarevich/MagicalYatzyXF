using NUnit.Framework;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.ViewModels;
using MagicalYatzyTests.ViewModelTests.Base;
using MagicalYatzyTests.ServiceTests;
using NSubstitute;

namespace MagicalYatzyTests.ViewModelTests
{
    public class LoginViewModelTests:BaseViewModelTests
    {
        LoginViewModel _sut;
        [SetUp]
        public override void Init()
        {
            base.Init();
            _sut = new LoginViewModel(playerServiceMock);
            _sut.SetNavigationService(navigationServiceMock);
        }

        [Test]
        public void SuccesfulLoginShouldCallBackNavigation()
        {
            playerServiceMock.LoginAsync(PlayerServiceTests.TestUserName, PlayerServiceTests.TestUserPassword).Returns(Task.FromResult(true));

            _sut.NewUsername = PlayerServiceTests.TestUserName;
            _sut.NewPassword = PlayerServiceTests.TestUserPassword;

            _sut.LoginCommand.Execute(null);

            navigationServiceMock.Received().CloseAsync();
        }
    }
}
