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
    }
}