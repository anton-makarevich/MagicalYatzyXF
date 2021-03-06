using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Ai.Extensions;
using Xunit;

namespace MagicalYatzyTests.Models.Game.Ai.Extensions
{
    public class AiHelpersTests
    {
        #region ResultHelpers
        [Fact]
        public void NumPairsReturnsCorrectAmountOfPairs()
        {
            var sut = new DieResult {DiceResults = new List<int> {1, 2, 1, 2, 1}};
            var result = sut.NumPairs();
            Assert.Equal(2,result);
            
            sut = new DieResult {DiceResults = new List<int> {1, 3, 1, 2, 1}};
            result = sut.NumPairs();
            Assert.Equal(1,result);
            
            sut = new DieResult {DiceResults = new List<int> {1, 3, 6, 2, 5}};
            result = sut.NumPairs();
            Assert.Equal(0,result);
        }
        
        [Fact]
        public void MinAllowedValueIsMoreThanZeroForEveryScoreExceptBonus()
        {
            var rule = new Rule(Rules.krSimple);
            foreach (var score in rule.ScoresForRule)
            {
                var result = new RollResult(score, rule.CurrentRule);
                
                Assert.True(result.MinAllowableValue()>0);
            }
        }
        #endregion
        
        #region DiceChangeLogic

        [Fact]
        public void IfBotGetsThreeInRowStartingWithOneItWantsToChangeSomethingToFour()
        {
            var diceResult = new DieResult() {DiceResults = new List<int>() {1, 2, 3, 5, 5}};

            var (oldValue, newValue) = diceResult.AiDecideDiceChange(true,false);
            
            Assert.Equal(5,oldValue);
            Assert.Equal(4,newValue);
        }
        
        [Fact]
        public void IfBotGetsThreeInRowStartingWithTwoItWantsToChangeSomethingToFive()
        {
            var diceResult = new DieResult() {DiceResults = new List<int>() {6, 2, 3, 4, 6}};

            var (oldValue, newValue) = diceResult.AiDecideDiceChange(true,false);
            
            Assert.Equal(6,oldValue);
            Assert.Equal(5,newValue);
        }
        
        [Fact]
        public void IfBotGetsThreeInRowStartingWithThreeItWantsToChangeSomethingToTwo()
        {
            var diceResult = new DieResult() {DiceResults = new List<int>() {1, 1, 3, 4, 5}};

            var (oldValue, newValue) = diceResult.AiDecideDiceChange(true,false);
            
            Assert.Equal(1,oldValue);
            Assert.Equal(2,newValue);
        }
        
        [Fact]
        public void IfBotGetsThreeInRowItWantsToChangeRepeatedValue()
        {
            var diceResult = new DieResult() {DiceResults = new List<int>() {4, 4, 3, 4, 5}};

            var (oldValue, newValue) = diceResult.AiDecideDiceChange(true,false);
            
            Assert.Equal(4,oldValue);
            Assert.Equal(2,newValue);
        }

        [Fact]
        public void IfBotGetsFourInRowStartingWithOneItWantsToChangeSomethingToFive()
        {
            var diceResult = new DieResult() {DiceResults = new List<int>() {1, 2, 3, 4, 6}};

            var (oldValue, newValue) = diceResult.AiDecideDiceChange(false,true);
            
            Assert.Equal(6,oldValue);
            Assert.Equal(5,newValue);
        }
        
        [Fact]
        public void IfBotGetsFourInRowStartingWithTwoItWantsToChangeSomethingToSix()
        {
            var diceResult = new DieResult() {DiceResults = new List<int>() {3, 2, 3, 4, 5}};

            var (oldValue, newValue) = diceResult.AiDecideDiceChange(false,true);
            
            Assert.Equal(3,oldValue);
            Assert.Equal(6,newValue);
        }
        
        [Fact]
        public void IfBotGetsFourInRowNotStartingWithThreeItWantsToChangeSomethingToTwo()
        {
            var diceResult = new DieResult() {DiceResults = new List<int>() {6, 1, 3, 4, 5}};

            var (oldValue, newValue) = diceResult.AiDecideDiceChange(false,true);
            
            Assert.Equal(1,oldValue);
            Assert.Equal(2,newValue);
        }
        
        [Fact]
        public void IfBotGetsFourInRowNotStartingWithThreeButDoesNotNeedLargeStraigntItWantsToChangeMinValueToMax()
        {
            var diceResult = new DieResult() {DiceResults = new List<int>() {6, 1, 3, 4, 5}};

            var (oldValue, newValue) = diceResult.AiDecideDiceChange(false,false);
            
            Assert.Equal(1,oldValue);
            Assert.Equal(6,newValue);
        }
        
        [Fact]
        public void IfBotGetsTwoSameItWantsToChangeSmallestToSameValue()
        {
            var diceResult = new DieResult() {DiceResults = new List<int>() {6, 1, 3, 3, 5}};

            var (oldValue, newValue) = diceResult.AiDecideDiceChange(false,false);
            
            Assert.Equal(1,oldValue);
            Assert.Equal(3,newValue);
        }
        
        [Fact]
        public void IfBotGetsTwoSameItWantsToChangeSmallestNotSameToSameValue()
        {
            var diceResult = new DieResult() {DiceResults = new List<int>() {6, 1, 1, 3, 5}};

            var (oldValue, newValue) = diceResult.AiDecideDiceChange(false,false);
            
            Assert.Equal(3,oldValue);
            Assert.Equal(1,newValue);
        }
        #endregion
    }
}