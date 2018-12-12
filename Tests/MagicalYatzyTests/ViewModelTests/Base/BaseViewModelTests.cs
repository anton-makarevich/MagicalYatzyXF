using NSubstitute;
using Sanet.MagicalYatzy.Services;

namespace MagicalYatzyTests.ViewModelTests.Base
{
    public abstract class BaseViewModelTests
    {
        protected IPlayerService playerServiceMock;
        protected INavigationService navigationServiceMock;
        protected BaseViewModelTests()
        {
            playerServiceMock = Substitute.For<IPlayerService>();
            navigationServiceMock = Substitute.For<INavigationService>();
        }
    }
}
