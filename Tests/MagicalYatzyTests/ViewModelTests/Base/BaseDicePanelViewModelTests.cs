using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;

namespace MagicalYatzyTests.ViewModelTests.Base
{
    public abstract class BaseDicePanelViewModelTests:BaseViewModelTests
    {
        protected IDicePanel dicePanelMock;
        public override void Init()
        {
            base.Init();
            dicePanelMock = Substitute.For<IDicePanel>();
        }
    }
}
