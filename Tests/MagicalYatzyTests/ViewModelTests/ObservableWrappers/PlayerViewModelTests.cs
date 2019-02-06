using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests.ObservableWrappers
{
    public class PlayerViewModelTests
    {
        private readonly PlayerViewModel _sut;
        private readonly IPlayer _player;
        
        public PlayerViewModelTests()
        {
            _player = Substitute.For<IPlayer>();
            _sut = new PlayerViewModel(_player);
        }

        [Fact]
        public void PlayerReturnsUnderlyingModel()
        {
            Assert.Equal(_player, _sut.Player);
        }
        
        [Fact]
        public void HasAllBasicPropertiesEqualToPlayerObject()
        {
            const string playerName = "New Player";
            const string profileImage = "Player.png";

            _player.Name.Returns(playerName);
            _player.ProfileImage.Returns(profileImage);
            
            Assert.Equal(playerName, _sut.Name);
            Assert.Equal(profileImage, _sut.Image);
        }
        
        [Fact]
        public void HasCorrectValueForDeleteImage()
        {
            const string deleteImage = "close.png";
            
            Assert.Equal(deleteImage, _sut.DeleteImage);
        }

        [Fact]
        public void DeletingPlayerInvokesDeletedEvent()
        {
            var playerDeletedCount = 0;

            _sut.PlayerDeleted += (sender, args) => { playerDeletedCount++; };

            _sut.DeleteCommand.Execute(null);
            
            Assert.Equal(1, playerDeletedCount);
        }
        
        [Fact]
        public void DeletingPlayerThatCanNotBeDeletedDoesNotInvokesDeletedEvent()
        {
            var playerDeletedCount = 0;

            _sut.CanBeDeleted = false;
            _sut.PlayerDeleted += (sender, args) => { playerDeletedCount++; };

            _sut.DeleteCommand.Execute(null);
            
            Assert.Equal(0, playerDeletedCount);
        }
    }
}