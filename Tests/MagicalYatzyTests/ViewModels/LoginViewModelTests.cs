using System.Threading.Tasks;
using MagicalYatzyTests.Services.Game;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Navigation;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MVVM.Core.Services;
using Xunit;

namespace MagicalYatzyTests.ViewModels;

public class LoginViewModelTests
{
    private readonly IPlayerService _playerServiceMock = Substitute.For<IPlayerService>();
    private readonly INavigationService _navigationServiceMock = Substitute.For<INavigationService>();

    private readonly IPlayer _playerStub = Substitute.For<IPlayer>();
        
    private readonly LoginViewModel _sut;

    public LoginViewModelTests()
    {
        _sut = new LoginViewModel(_playerServiceMock);
        _sut.SetNavigationService(_navigationServiceMock);
    }

    [Fact]
    public void SuccessfulLoginCallsBackNavigation()
    {
        _playerServiceMock.LoginAsync(PlayerServiceTests.TestUserName, PlayerServiceTests.TestUserPassword).Returns(Task.FromResult(_playerStub));

        _sut.NewUsername = PlayerServiceTests.TestUserName;
        _sut.NewPassword = PlayerServiceTests.TestUserPassword;

        _sut.LoginCommand.Execute(null);

        _navigationServiceMock.Received().CloseAsync();
    }
        
    [Fact]
    public void SuccessfulLoginCallsBackNavigationAndProvidesLoggedInPlayerAsResultIfItIsExpected()
    {
        _sut.ExpectsResult = true;
        var onResultCalledCount = 0;
        _sut.OnResult += (sender, o) =>
        {
            onResultCalledCount++;
            Assert.Equal(_playerStub, o);
        }; 
            
        _playerServiceMock.LoginAsync(PlayerServiceTests.TestUserName, PlayerServiceTests.TestUserPassword).Returns(Task.FromResult(_playerStub));

        _sut.NewUsername = PlayerServiceTests.TestUserName;
        _sut.NewPassword = PlayerServiceTests.TestUserPassword;

        _sut.LoginCommand.Execute(null);

        _navigationServiceMock.Received().CloseAsync();
        Assert.Equal(1,onResultCalledCount);
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