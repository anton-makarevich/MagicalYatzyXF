using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels;
using NSubstitute;
using Xunit;
using Sanet.MagicalYatzy.Resources;

namespace MagicalYatzyTests.ViewModelTests
{
    public class LobbyViewModelTests
    {
        private readonly IDicePanel _dicePanelMock;
        private readonly LobbyViewModel _sut;

        public LobbyViewModelTests()
        {
            _dicePanelMock = Substitute.For<IDicePanel>();
            _sut = new LobbyViewModel(_dicePanelMock);
        }

        [Fact]
        public void LobbyViewModelHasDicePanel()
        {
            Assert.NotNull(_sut.DicePanel);
        }

        [Fact]
        public void PanelTitlesAreCorrect()
        {
            Assert.Equal(Strings.PlayersLabel.ToUpper(), _sut.PlayersTitle);
            Assert.Equal(Strings.RulesLabel.ToUpper(), _sut.RulesTitle);
        }
    }
}
