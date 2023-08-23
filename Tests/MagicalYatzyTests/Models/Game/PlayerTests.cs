using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Utils;
using Xunit;

namespace MagicalYatzyTests.Models.Game
{
    public class PlayerTests
    {
        private readonly Player _sut = new();

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
            var artifact2 = new Artifact(Artifacts.RollReset);

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
            Assert.Contains("Bot", sut.Name);
        }
        
        [Fact]
        public void LocalPlayerHasIsUnknown_WhenName_IsNot_Passed()
        {
            _sut.Name.Should().Be("Unknown");
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
        public void CorrectlyCalculatesNumericScores()
        {
            // Arrange
            var rule = new Rule(Rules.krBaby);
            _sut.PrepareForGameStart(rule);
            
            // Act
            foreach (var score in rule.ScoresForRule)
            {
                var result = _sut.Results.FirstOrDefault(r => r.ScoreType == score);
                if (result != null && result.ScoreType != Scores.Ones)
                {
                    result.Value = result.MaxValue;
                }
            }
            
            // Assert
            Assert.Equal(100,_sut.TotalNumeric);
        }
        
        [Fact]
        public void CorrectlyCalculatesMaxRemainingNumericScore()
        {
            // Arrange
            var rule = new Rule(Rules.krBaby);
            _sut.PrepareForGameStart(rule);
            
            // Act
            foreach (var score in rule.ScoresForRule)
            {
                var result = _sut.Results.FirstOrDefault(r => r.ScoreType == score);
                if (result != null && result.ScoreType == Scores.Ones)
                {
                    result.Value = result.MaxValue;
                }
            }
            
            // Assert
            Assert.Equal(100,_sut.MaxRemainingNumeric);
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
        public void FillsCorrectPossibleValueForChanceResult()
        {
            // Arrange
            var rule = new Rule(Rules.krSimple);
            const Scores scoreToCheck = Scores.Chance;
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
        
        [Fact]
        public void FillsCorrectPossibleValueForFullHouseResult()
        {
            // Arrange
            var rule = new Rule(Rules.krSimple);
            const Scores scoreToCheck = Scores.FullHouse;
            _sut.PrepareForGameStart(rule);
            
            // Act
            _sut.CheckRollResults(new DieResult(){DiceResults = new List<int>(){3,3,2,2,3}}, rule);
            
            // Assert
            Assert.Equal(25,_sut.Results.First(r=>r.ScoreType == scoreToCheck).PossibleValue);
        }
        
        [Fact]
        public void FillsYatzyJokerAsPossibleValueForFullHouseResultWhenYatzyIsAlreadyFilledForRulesWithExtendedBonus()
        {
            // Arrange
            var extendedRules = new[]
            {
                new Rule(Rules.krExtended),
                new Rule(Rules.krMagic)
            };
            foreach (var rule in extendedRules)
            {
                const Scores scoreToCheck = Scores.FullHouse;
                _sut.PrepareForGameStart(rule);
                _sut.Results.Single(r => r.ScoreType == Scores.Kniffel).Value = 50;
                _sut.Results.Single(r => r.ScoreType == Scores.Threes).Value = 6;
            
                // Act
                _sut.CheckRollResults(new DieResult(){DiceResults = new List<int>(){3,3,3,3,3}}, rule);
            
                // Assert
                Assert.Equal(25,_sut.Results.First(r=>r.ScoreType == scoreToCheck).PossibleValue);
            }
        }
        
        [Fact]
        public void FillsCorrectPossibleValueForSmallStraightResult()
        {
            // Arrange
            var rule = new Rule(Rules.krSimple);
            const Scores scoreToCheck = Scores.SmallStraight;
            _sut.PrepareForGameStart(rule);
            
            // Act
            _sut.CheckRollResults(new DieResult(){DiceResults = new List<int>(){1,3,2,4,3}}, rule);
            
            // Assert
            Assert.Equal(30,_sut.Results.First(r=>r.ScoreType == scoreToCheck).PossibleValue);
        }
        
        [Fact]
        public void FillsYatzyJokerAsPossibleValueForSmallStraightResultWhenYatzyIsAlreadyFilledForRulesWithExtendedBonus()
        {
            // Arrange
            var extendedRules = new[]
            {
                new Rule(Rules.krExtended),
                new Rule(Rules.krMagic)
            };
            foreach (var rule in extendedRules)
            {
                const Scores scoreToCheck = Scores.SmallStraight;
                _sut.PrepareForGameStart(rule);
                _sut.Results.Single(r => r.ScoreType == Scores.Kniffel).Value = 50;
                _sut.Results.Single(r => r.ScoreType == Scores.Threes).Value = 6;
            
                // Act
                _sut.CheckRollResults(new DieResult(){DiceResults = new List<int>(){3,3,3,3,3}}, rule);
            
                // Assert
                Assert.Equal(30,_sut.Results.First(r=>r.ScoreType == scoreToCheck).PossibleValue);
            }
        }
        
        [Fact]
        public void UseArtifactChangeDecreaseArtifactsCount()
        {
            var artifact = new Artifact(Artifacts.ManualSet);

            _sut.AvailableMagicalArtifacts = new List<Artifact> {artifact};
            _sut.PrepareForGameStart(new Rule(Rules.krMagic));
            _sut.UseArtifact(Artifacts.ManualSet);
            
            Assert.True(_sut.MagicalArtifactsForGame.First(f=>f.Type == Artifacts.ManualSet).IsUsed);
        }

        [Fact]
        public void ReturnsCorrectTotalScoreDependingOnRules()
        {
            foreach (var ruleType in EnumUtils.GetValues<Rules>())
            {
                var rule = new Rule(ruleType);
                _sut.PrepareForGameStart(rule);

                foreach (var score in EnumUtils.GetValues<Scores>())
                {
                    var result = _sut.Results.LastOrDefault(r => r.ScoreType == score);
                    if (result != null)
                        result.Value = result.MaxValue;
                }
           
                var expectedTotal = _sut.Results
                                        .Where(r => r.HasValue).Select(r => r.Value)
                                        .Sum() + _sut.Results.Count(f=>f.HasBonus)*100;
                
                // Assert
                Assert.Equal(expectedTotal,_sut.Total);
            }
        }
        
        [Fact]
        public void CanUseArtifactReturnsTrueIfArtifactIsAvailableAndNotUsed()
        {
            var artifact = new Artifact(Artifacts.ManualSet);

            _sut.AvailableMagicalArtifacts = new List<Artifact> {artifact};
            _sut.PrepareForGameStart(new Rule(Rules.krMagic));
            
            Assert.True(_sut.CanUseArtifact(Artifacts.ManualSet));
            Assert.False(_sut.CanUseArtifact(Artifacts.MagicalRoll));
            Assert.False(_sut.CanUseArtifact(Artifacts.RollReset));
            
            _sut.UseArtifact(Artifacts.ManualSet);
            
            Assert.False(_sut.CanUseArtifact(Artifacts.ManualSet));
        }

        [Fact]
        public void BotPlayerHasDecisionMaker()
        {
            var sut = new Player(PlayerType.AI);
            
            Assert.NotNull(sut.DecisionMaker);
        }

        [Fact]
        public void PlayerIsNotEqualToNotPlayer()
        {
            var notPlayer = new object();

            var isEqual = _sut.Equals(notPlayer);
            
            Assert.False(isEqual);
        }
    }
}
