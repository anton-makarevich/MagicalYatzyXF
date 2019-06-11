using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using System.Linq;
using Xunit;

namespace MagicalYatzyTests.Models.Game
{
    public class DiceResultTests
    {
        [Fact]
        public void CorrectlyCalculatesAmountOfValuesInRowAndFirstValueOfSequence()
        {
            var sut = new DieResult {DiceResults = new List<int> {3, 2, 4, 2, 4}};
            var (firstValue, numberOfValuesInRow) = sut.CalculateInRowDice();
            Assert.Equal(2,firstValue);
            Assert.Equal(3,numberOfValuesInRow);
            
            sut = new DieResult {DiceResults = new List<int> {3, 2, 4, 2, 1}};
            (firstValue, numberOfValuesInRow) = sut.CalculateInRowDice();
            Assert.Equal(1,firstValue);
            Assert.Equal(4,numberOfValuesInRow);
            
            sut = new DieResult {DiceResults = new List<int> {3, 5, 4, 2, 6}};
            (firstValue, numberOfValuesInRow) = sut.CalculateInRowDice();
            Assert.Equal(2,firstValue);
            Assert.Equal(5,numberOfValuesInRow);
            
            sut = new DieResult {DiceResults = new List<int> {3, 1, 4, 6, 6}};
            (firstValue, numberOfValuesInRow) = sut.CalculateInRowDice();
            Assert.Equal(3,firstValue);
            Assert.Equal(2,numberOfValuesInRow);
            
            sut = new DieResult {DiceResults = new List<int> {1, 2, 4, 5, 6}};
            (firstValue, numberOfValuesInRow) = sut.CalculateInRowDice();
            Assert.Equal(4,firstValue);
            Assert.Equal(3,numberOfValuesInRow);
            
            sut = new DieResult {DiceResults = new List<int> {1, 2, 3, 5, 6}};
            (firstValue, numberOfValuesInRow) = sut.CalculateInRowDice();
            Assert.Equal(1,firstValue);
            Assert.Equal(3,numberOfValuesInRow);
        }
        
        [Fact]
        public void CorrectlyCalculatesDiceOccurrencesInDiceResult()
        {
            var resultsToTest = new[]
            {
                new List<int>() {4, 4, 3, 4, 5},
                new List<int>() {2, 4, 3, 4, 5},
                new List<int>() {4, 4, 4, 4, 5},
                new List<int>() {1, 5, 5, 5, 6},
            };
            foreach (var resultToTest in resultsToTest)
            {
                var sut = new DieResult() {DiceResults = resultToTest};
                var occurrences = sut.CalculateDiceOccurrences();
                foreach (var (diceValue, amountOfDice) in occurrences)
                    Assert.Equal(sut.DiceResults.Count(i=>i==diceValue),amountOfDice);
            }
        }

        [Fact]
        public void TotalIsEqualToZeroIfResultsAreNotDefines()
        {
            var sut = new DieResult();
            
            Assert.Equal(0,sut.Total);
        }
        
        [Fact]
        public void NumberOfDiceIsEqualToZeroIfResultsAreNotDefines()
        {
            var sut = new DieResult();
            
            Assert.Equal(0,sut.NumDice);
        }
    }
}