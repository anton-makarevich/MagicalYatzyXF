using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;

namespace MagicalYatzyTests.ViewModelTests.Base
{
    public abstract class BaseDicePanelViewModelTests
    {
        private readonly IDicePanel _dicePanelMock;
        protected BaseDicePanelViewModelTests()
        {
            _dicePanelMock = Substitute.For<IDicePanel>();
        }
    }
}
