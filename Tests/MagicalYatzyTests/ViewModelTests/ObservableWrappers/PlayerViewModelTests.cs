using System.Collections.Generic;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Xunit;
using Sanet.MagicalYatzy.Services;

namespace MagicalYatzyTests.ViewModelTests.ObservableWrappers
{
    public class PlayerViewModelTests
    {
        private readonly PlayerViewModel _sut;
        private readonly IPlayer _player;
        private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
        
        public PlayerViewModelTests()
        {
            _player = Substitute.For<IPlayer>();
            _sut = new PlayerViewModel(_player, _localizationService);
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

        [Fact]
        public void HasRollResultsCollection()
        {
            Assert.NotNull(_sut.Results);
            Assert.Equal(_sut.Player.Results.Count, _sut.Results.Count);
        }

        [Fact]
        public void ApplyRollResultUpdatesCorrespondingPlayersResult()
        {
            const Scores scoreType = Scores.Ones;
            var playersResult = new RollResult(scoreType,Rules.krExtended);
            _player.Results.Returns(new List<RollResult>() {playersResult});
            
            var newResult = Substitute.For<IRollResult>();
            newResult.PossibleValue.Returns(5);
            newResult.HasBonus.Returns(true);
            newResult.ScoreType.Returns(scoreType);
            
            _sut.ApplyRollResult(newResult);

            Assert.Equal(newResult.PossibleValue,playersResult.Value);
            Assert.Equal(newResult.HasBonus, playersResult.HasBonus);
        }
        
        [Fact]
        public void ApplyRollResultUpdatesPlayersTotal()
        {
            const Scores scoreType = Scores.Kniffel;
            var totalUpdatedTimes = 0;
            var newResult = Substitute.For<IRollResult>();
            newResult.PossibleValue.Returns(50);
            newResult.HasBonus.Returns(true);
            newResult.ScoreType.Returns(scoreType);
            _sut.PropertyChanged += (sender, args) => { totalUpdatedTimes++; };
            
            _sut.ApplyRollResult(newResult);

            Assert.Equal(1,totalUpdatedTimes);
        }

        [Fact]
        public void TotalReturnsPlayersTotalScore()
        {
            _player.Total.Returns(45);
            
            Assert.Equal(_player.Total, _sut.Total);
        }
    }
}