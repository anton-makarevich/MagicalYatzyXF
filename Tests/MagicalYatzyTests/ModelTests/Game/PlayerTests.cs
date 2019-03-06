using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void ReturnsIfScoreIsFilled()
        {
            const Scores filledScore = Scores.Ones;
            _sut.PrepareForGameStart(new Rule(Rules.krMagic));
            _sut.Results.First(f => f.ScoreType == filledScore).Value = 5;

            foreach (var scoreType in EnumUtils.GetValues<Scores>())
            {
                if (scoreType != filledScore)
                    Assert.False(_sut.IsScoreFilled(scoreType));
            }
            
            Assert.True(_sut.IsScoreFilled(filledScore));
        }

        [Fact]
        public void FillsCorrectPossibleValueForNumericResult()
        {
            // Arrange
            var rule = new Rule(Rules.krBaby);
            const Scores scoreToCheck = Scores.Sixs;
            _sut.PrepareForGameStart(rule);
            
            // Act
            _sut.CheckRollResults(new DieResult(){DiceResults = new List<int>(){3,5,6,3,6}}, rule);
            
            // Assert
            Assert.Equal(12,_sut.Results.First(r=>r.ScoreType == scoreToCheck).PossibleValue);
        }
        
        [Fact]
        public void FillsCorrectPossibleValueForThreeOfAKindResult()
        {
            // Arrange
            var rule = new Rule(Rules.krSimple);
            const Scores scoreToCheck = Scores.ThreeOfAKind;
            _sut.PrepareForGameStart(rule);
            
            // Act
            _sut.CheckRollResults(new DieResult(){DiceResults = new List<int>(){3,3,6,3,6}}, rule);
            
            // Assert
            Assert.Equal(21,_sut.Results.First(r=>r.ScoreType == scoreToCheck).PossibleValue);
        }
        
        [Fact]
        public void FillsCorrectPossibleValueForFourOfAKindResult()
        {
            // Arrange
            var rule = new Rule(Rules.krSimple);
            const Scores scoreToCheck = Scores.FourOfAKind;
            _sut.PrepareForGameStart(rule);
            
            // Act
            _sut.CheckRollResults(new DieResult(){DiceResults = new List<int>(){3,3,3,3,6}}, rule);
            
            // Assert
            Assert.Equal(18,_sut.Results.First(r=>r.ScoreType == scoreToCheck).PossibleValue);
        }
        
        [Fact]
        public void FillsCorrectPossibleValueForChanseResult()
        {
            // Arrange
            var rule = new Rule(Rules.krSimple);
            const Scores scoreToCheck = Scores.Total;
            _sut.PrepareForGameStart(rule);
            
            // Act
            _sut.CheckRollResults(new DieResult(){DiceResults = new List<int>(){3,3,3,3,6}}, rule);
            
            // Assert
            Assert.Equal(18,_sut.Results.First(r=>r.ScoreType == scoreToCheck).PossibleValue);
        }
        
        [Fact]
        public void FillsCorrectPossibleValueForYatzyResult()
        {
            // Arrange
            var rule = new Rule(Rules.krSimple);
            const Scores scoreToCheck = Scores.Kniffel;
            _sut.PrepareForGameStart(rule);
            
            // Act
            _sut.CheckRollResults(new DieResult(){DiceResults = new List<int>(){3,3,3,3,3}}, rule);
            
            // Assert
            Assert.Equal(50,_sut.Results.First(r=>r.ScoreType == scoreToCheck).PossibleValue);
        }
    }
}
