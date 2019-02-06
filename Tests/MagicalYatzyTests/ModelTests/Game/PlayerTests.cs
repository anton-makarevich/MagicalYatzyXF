using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Xunit;
using Sanet.MagicalYatzy.Utils;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Resources;

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
            _sut.PrepareForGameStart(new Rule(Rules.krSimple));
            Assert.NotNull(_sut.Results);
            Assert.NotEmpty(_sut.Results);
            foreach (var rollResult in _sut.Results)
            {
                Assert.False(rollResult.HasValue);
            }
        }

        [Fact]
        public void DoesNotHaveArtifactsIfRulesAreNotMagical()
        {
            _sut.PrepareForGameStart(new Rule(Rules.krSimple));
            Assert.Null(_sut.MagicalArtifactsForGame);
        }
        
        [Fact]
        public void DoesNotHaveArtifactsIfRulesAreMagicalButAvailableArtifactsAreNotSet()
        {
            _sut.PrepareForGameStart(new Rule(Rules.krMagic));
            Assert.Null(_sut.MagicalArtifactsForGame);
        }

        [Fact]
        public void HasArtifactsIfRulesAreMagicalAndAvailableArtifactsAreSet()
        {
            var artifact1 = new Artifact(Artifacts.ManualSet);
            var artifact2 = new Artifact(Artifacts.FourthRoll);

            _sut.AvailableMagicalArtifacts = new List<Artifact> {artifact1, artifact2};
            _sut.PrepareForGameStart(new Rule(Rules.krMagic));
            
            Assert.NotNull(_sut.MagicalArtifactsForGame);
            Assert.Contains(artifact1, _sut.MagicalArtifactsForGame);
            Assert.Contains(artifact2, _sut.MagicalArtifactsForGame);
        }

        [Fact]
        public void PrepareForGameResetsToFirstRoll()
        {
            _sut.PrepareForGameStart(new Rule(Rules.krMagic));
            Assert.Equal(1, _sut.Roll);
        }
        
        [Fact]
        public void PrepareForGameSetsInGameId()
        {
            _sut.PrepareForGameStart(new Rule(Rules.krMagic));
            Assert.NotEmpty(_sut.InGameId);
        }

        [Fact]
        public void ReturnsResultForRequestedScore()
        {
            _sut.PrepareForGameStart(new Rule(Rules.krMagic));
            const Scores score = Scores.Ones;
            var result = _sut.GetResultForScore(score);
            Assert.NotNull(result);
            Assert.Equal(score, result.ScoreType);
        }
        
        [Fact]
        public void AiPlayerHasBotName()
        {
            var sut = new Player(PlayerType.AI);
            Assert.Contains(Strings.BotNameDefault, sut.Name);
        }
        
        [Fact]
        public void LocalPlayerHasPlayerName()
        {
            Assert.Contains(Strings.PlayerNameDefault, _sut.Name);
        }
    }
}
