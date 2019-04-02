using System.Linq;
using Sanet.MagicalYatzy.Models.Game;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class RuleTests
    {
        [Fact]
        public void RuleHasTypePassedToConstructor()
        {
            const Rules ruleType = Rules.krMagic;
            var sut = new Rule(ruleType);
            
            Assert.Equal(ruleType, sut.CurrentRule);
        }

        [Fact]
        public void ScoreForRulesReturnsNumericAndKniffelForBabyRules()
        {
            var sut = new Rule(Rules.krBaby);
            var babyScores = new []
            {
                Scores.Ones,
                Scores.Twos,
                Scores.Threes,
                Scores.Fours,
                Scores.Fives,
                Scores.Sixs,
                Scores.Kniffel
            };

            var scores = sut.ScoresForRule.ToList();
            
            Assert.Equal(7, scores.Count);

            foreach (var babyScore in babyScores)
            {
                Assert.Contains(babyScore, scores);
            }
        }
        
        [Fact]
        public void ScoreForRulesReturnsNumericPokerHandsAndKniffelForSimpleRules()
        {
            var sut = new Rule(Rules.krSimple);
            var simpleScores = new []
            {
                Scores.Ones,
                Scores.Twos,
                Scores.Threes,
                Scores.Fours,
                Scores.Fives,
                Scores.Sixs,
                Scores.ThreeOfAKind,
                Scores.FourOfAKind,
                Scores.FullHouse,
                Scores.SmallStraight,
                Scores.LargeStraight,
                Scores.Chance,
                Scores.Kniffel
            };

            var scores = sut.ScoresForRule.ToList();
            
            Assert.Equal(13, scores.Count);

            foreach (var babyScore in simpleScores)
            {
                Assert.Contains(babyScore, scores);
            }
        }
        
        [Fact]
        public void ScoreForRulesReturnsNumericBonusPokerHandsAndKniffelForExtendedRules()
        {
            var sut = new Rule(Rules.krExtended);
            var extendedScores = new []
            {
                Scores.Ones,
                Scores.Twos,
                Scores.Threes,
                Scores.Fours,
                Scores.Fives,
                Scores.Sixs,
                Scores.Bonus,
                Scores.ThreeOfAKind,
                Scores.FourOfAKind,
                Scores.FullHouse,
                Scores.SmallStraight,
                Scores.LargeStraight,
                Scores.Chance,
                Scores.Kniffel
            };

            var scores = sut.ScoresForRule.ToList();
            
            Assert.Equal(14, scores.Count);

            foreach (var babyScore in extendedScores)
            {
                Assert.Contains(babyScore, scores);
            }
        }
        
        [Fact]
        public void ScoreForRulesReturnsNumericBonusPokerHandsAndKniffelForMagicRules()
        {
            var sut = new Rule(Rules.krMagic);
            var magicScores = new []
            {
                Scores.Ones,
                Scores.Twos,
                Scores.Threes,
                Scores.Fours,
                Scores.Fives,
                Scores.Sixs,
                Scores.Bonus,
                Scores.ThreeOfAKind,
                Scores.FourOfAKind,
                Scores.FullHouse,
                Scores.SmallStraight,
                Scores.LargeStraight,
                Scores.Chance,
                Scores.Kniffel
            };

            var scores = sut.ScoresForRule.ToList();
            
            Assert.Equal(14, scores.Count);

            foreach (var babyScore in magicScores)
            {
                Assert.Contains(babyScore, scores);
            }
        }
        
        [Fact]
        public void ScoreForRulesReturnsNumericBonusPokerHandsAndKniffelForStandardRules()
        {
            var sut = new Rule(Rules.krStandard);
            var magicScores = new []
            {
                Scores.Ones,
                Scores.Twos,
                Scores.Threes,
                Scores.Fours,
                Scores.Fives,
                Scores.Sixs,
                Scores.Bonus,
                Scores.ThreeOfAKind,
                Scores.FourOfAKind,
                Scores.FullHouse,
                Scores.SmallStraight,
                Scores.LargeStraight,
                Scores.Chance,
                Scores.Kniffel
            };

            var scores = sut.ScoresForRule.ToList();
            
            Assert.Equal(14, scores.Count);

            foreach (var babyScore in magicScores)
            {
                Assert.Contains(babyScore, scores);
            }
        }
        
        [Fact]
        public void MaxRoundEqualToSevenForBabyRules()
        {
            var sut = new Rule(Rules.krBaby);
       
            Assert.Equal(7, sut.MaxRound);
        }
        
        [Fact]
        public void MaxRoundEqualToThirteenForSimpleRules()
        {
            var sut = new Rule(Rules.krSimple);
       
            Assert.Equal(13, sut.MaxRound);
        }
        
        [Fact]
        public void MaxRoundEqualToThirteenForStandardRules()
        {
            var sut = new Rule(Rules.krStandard);
       
            Assert.Equal(13, sut.MaxRound);
        }
        
        [Fact]
        public void MaxRoundEqualToThirteenForExtendedRules()
        {
            var sut = new Rule(Rules.krExtended);
       
            Assert.Equal(13, sut.MaxRound);
        }
        
        [Fact]
        public void MaxRoundEqualToThirteenForMagicRules()
        {
            var sut = new Rule(Rules.krMagic);
       
            Assert.Equal(13, sut.MaxRound);
        }
        
        [Fact]
        public void DoesNotHaveExtendedBonusForBabyRules()
        {
            var sut = new Rule(Rules.krBaby);
       
            Assert.False(sut.HasExtendedBonuses);
        }
        
        [Fact]
        public void DoesNotHaveExtendedBonusForSimpleRules()
        {
            var sut = new Rule(Rules.krSimple);
       
            Assert.False(sut.HasExtendedBonuses);
        }
        
        [Fact]
        public void DoesNotHaveExtendedBonusForStandardRules()
        {
            var sut = new Rule(Rules.krStandard);
       
            Assert.False(sut.HasExtendedBonuses);
        }
        
        [Fact]
        public void HasExtendedBonusForExtendedRules()
        {
            var sut = new Rule(Rules.krExtended);
       
            Assert.True(sut.HasExtendedBonuses);
        }
        
        [Fact]
        public void HasExtendedBonusForMagicRules()
        {
            var sut = new Rule(Rules.krMagic);
       
            Assert.True(sut.HasExtendedBonuses);
        }
        
        [Fact]
        public void DoesNotHaveStandardBonusForBabyRules()
        {
            var sut = new Rule(Rules.krBaby);
       
            Assert.False(sut.HasStandardBonus);
        }
        
        [Fact]
        public void DoesNotHaveStandardBonusForSimpleRules()
        {
            var sut = new Rule(Rules.krSimple);
       
            Assert.False(sut.HasStandardBonus);
        }
        
        [Fact]
        public void HasStandardBonusForStandardRules()
        {
            var sut = new Rule(Rules.krStandard);
       
            Assert.True(sut.HasStandardBonus);
        }
        
        [Fact]
        public void HasStandardBonusForExtendedRules()
        {
            var sut = new Rule(Rules.krExtended);
       
            Assert.True(sut.HasStandardBonus);
        }
        
        [Fact]
        public void HasStandardBonusForMagicRules()
        {
            var sut = new Rule(Rules.krMagic);
       
            Assert.True(sut.HasStandardBonus);
        }
        
        [Fact]
        public void ReturnsScoreBStringForBabyRules()
        {
            var sut = new Rule(Rules.krBaby);
       
            Assert.Equal("ScoresB",sut.ToString());
        }
        
        [Fact]
        public void ReturnsScoreStringForSimpleRules()
        {
            var sut = new Rule(Rules.krSimple);
       
            Assert.Equal("Scores",sut.ToString());
        }
        
        [Fact]
        public void ReturnsScoreSStringForStandardRules()
        {
            var sut = new Rule(Rules.krStandard);
       
            Assert.Equal("ScoresS",sut.ToString());
        }
        
        [Fact]
        public void ReturnsScoreEStringForExtendedRules()
        {
            var sut = new Rule(Rules.krExtended);
       
            Assert.Equal("ScoresE",sut.ToString());
        }
        
        [Fact]
        public void ReturnsScoreMStringForMagicRules()
        {
            var sut = new Rule(Rules.krMagic);
       
            Assert.Equal("ScoresM",sut.ToString());
        }
    }
}