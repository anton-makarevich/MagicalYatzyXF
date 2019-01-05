using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Xunit;
using Sanet.MagicalYatzy.Utils;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game.Magical;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class PlayerTests
    {
        private readonly Player _sut;

        public PlayerTests()
        {
            _sut = new Player();
        }

        [Fact]
        public void DefaultPlayerIsHuman()
        {
            Assert.True(_sut.IsHuman);
        }

        [Fact]
        public void PlayerTypeIsSetInConstructor()
        {
            var playerTypes = EnumUtils.GetValues<PlayerType>();
            foreach (var type in playerTypes)
            {
                var sut = new Player(type);
                Assert.Equal(type, sut.Type);
            }
        }

        [Fact]
        public void PlayerWithAiTypeIsBot()
        {
            var sut = new Player(PlayerType.AI);
            Assert.True(sut.IsBot);
        }

        [Fact]
        public void PlayersHashcodeIsBasedOnNameAndEncodedPassword()
        {
            var expectedHashcode = $"player{_sut.Name}{_sut.Password.Decrypt(33)}".GetHashCode();
            Assert.Equal(expectedHashcode, _sut.GetHashCode());
        }

        [Fact]
        public void PrepareForGameStartInitializesResults()
        {
            _sut.PrepareForGameStart();
            Assert.NotNull(_sut.Results);
            Assert.NotEmpty(_sut.Results);
            foreach (var rollResult in _sut.Results)
            {
                Assert.False(rollResult.HasValue);
            }
        }

        [Fact]
        public void DoesNotHaveArtifactsIfNotPassedToPrepareForGame()
        {
            _sut.PrepareForGameStart();
            Assert.Null(_sut.MagicalArtifacts);
        }

        [Fact]
        public void HasArtifactsPassedToPrepareToPrepareForGame()
        {
            var artifact1 = new Artifact(Artifacts.ManualSet);
            var artifact2 = new Artifact(Artifacts.FourthRoll);
            var artifacts = new List<Artifact> {artifact1, artifact2};
            
            _sut.PrepareForGameStart(artifacts);
            
            Assert.NotNull(_sut.MagicalArtifacts);
            Assert.Contains(artifact1, _sut.MagicalArtifacts);
            Assert.Contains(artifact2, _sut.MagicalArtifacts);
        }

        [Fact]
        public void PrepareForGameResetsToFirstRoll()
        {
            _sut.PrepareForGameStart();
            Assert.Equal(1, _sut.Roll);
        }
    }
}
