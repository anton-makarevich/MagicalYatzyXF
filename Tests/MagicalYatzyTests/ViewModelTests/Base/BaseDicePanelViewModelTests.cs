using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;

namespace MagicalYatzyTests.ViewModelTests.Base
{
    public abstract class BaseDicePanelViewModelTests:BaseViewModelTests
    {
        protected IDicePanel dicePanelMock;
        protected BaseDicePanelViewModelTests()
        {
            dicePanelMock = Substitute.For<IDicePanel>();
        }
    }
}
