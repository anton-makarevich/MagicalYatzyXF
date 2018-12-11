using NSubstitute;
using Sanet.MagicalYatzy.Services;

namespace MagicalYatzyTests.ViewModelTests.Base
{
    public abstract class BaseViewModelTests
    {
        protected IPlayerService playerServiceMock;
        protected INavigationService navigationServiceMock;
        public virtual void Init()
        {
            playerServiceMock = Substitute.For<IPlayerService>();
            navigationServiceMock = Substitute.For<INavigationService>();
        }
    }
}
