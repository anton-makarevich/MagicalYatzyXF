using System.Collections.Generic;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Xunit;

namespace MagicalYatzyTests.ViewModels.ObservableWrappers
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
        public void HasCorrectIsMyTurnValue()
        {
            _player.IsMyTurn.Returns(true);
            
            Assert.True(_sut.IsMyTurn);
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
            
            _sut.ApplyRollResult(new RollResultEventArgs(_player,5, scoreType, true));

            Assert.Equal(5,playersResult.Value);
            Assert.True(playersResult.HasBonus);
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
            
            _sut.ApplyRollResult(new RollResultEventArgs(_player, 50, scoreType, true));

            Assert.Equal(1,totalUpdatedTimes);
        }

        [Fact]
        public void TotalReturnsPlayersTotalScore()
        {
            _player.Total.Returns(45);
            
            Assert.Equal(_player.Total, _sut.Total);
        }

        [Fact]
        public void RefreshMethodRaisesResultsAndIsMyTurnProperties()
        {
            var resultsUpdatedTimes = 0;
            var isMyTurnUpdatedTimes = 0;
            _sut.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_sut.Results))
                    resultsUpdatedTimes++;
                if (args.PropertyName == nameof(_sut.IsMyTurn))
                    isMyTurnUpdatedTimes++;
            };
            
            _sut.Refresh();
            
            Assert.Equal(1,resultsUpdatedTimes);
            Assert.Equal(1, isMyTurnUpdatedTimes);
        }
    }
}