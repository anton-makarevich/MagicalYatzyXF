using System.Linq;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels;
using NSubstitute;
using Xunit;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

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
        
        [Fact]
        public void CallingDeleteOnPlayerViewModelRemovesItFromList()
        {
            var newPlayerVm = new PlayerViewModel(Substitute.For<IPlayer>());

            _sut.AttachHandlers();
            _sut.AddPlayer(newPlayerVm);
            var initialPlayerCount = _sut.Players.Count;
            
            newPlayerVm.DeleteCommand.Execute(null);

            Assert.Equal(initialPlayerCount - 1, _sut.Players.Count);
        }
        
        [Fact]
        public void SinglePlayerCanNotBeDeleted()
        {
            _sut.AttachHandlers();

            Assert.Single(_sut.Players);
            Assert.False(_sut.Players.First().CanBeDeleted);
        }

        [Fact]
        public void PlayersCouldBeDeletedIfThereAreMoreThanTwoInTheList()
        {
            var newPlayerVm = new PlayerViewModel(Substitute.For<IPlayer>());

            _sut.AttachHandlers();
            _sut.AddPlayer(newPlayerVm);
            
            Assert.Equal(2, _sut.Players.Count);
            foreach (var playerViewModel in _sut.Players)
            {
                Assert.True(playerViewModel.CanBeDeleted);
            }
        }
        
        [Fact]
        public void PlayersCanNotBeDeletedIfOnlyOneRemainsAfterDeletion()
        {
            var newPlayerVm = new PlayerViewModel(Substitute.For<IPlayer>());

            _sut.AttachHandlers();
            _sut.AddPlayer(newPlayerVm);
            newPlayerVm.DeleteCommand.Execute(null);
            
            Assert.Single(_sut.Players);
            Assert.False(_sut.Players.First().CanBeDeleted);
        }

        [Fact]
        public void AddBotImageHasCorrectValue()
        {
            Assert.Equal("AddBot.png", _sut.AddBotImage);
        }
    }
}
