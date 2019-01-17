using System.Linq;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels;
using NSubstitute;
using Xunit;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services.Game;

namespace MagicalYatzyTests.ViewModelTests
{
    public class LobbyViewModelTests
    {
        private readonly LobbyViewModel _sut;
        private readonly IPlayerService _playerService;

        public LobbyViewModelTests()
        {
            _playerService = Substitute.For<IPlayerService>();
            var dicePanelMock = Substitute.For<IDicePanel>();
            _sut = new LobbyViewModel(dicePanelMock, _playerService);
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

        [Fact]
        public void CurrentLoggedInPlayerAddedToPlayersList()
        {
            _sut.AttachHandlers();
            
            Assert.NotEmpty(_sut.Players);
            Assert.Equal(_playerService.CurrentPlayer.Name, _sut.Players.First().Name);
        }
    }
}
