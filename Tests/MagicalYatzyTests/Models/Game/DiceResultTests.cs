using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using System.Linq;
using Xunit;

namespace MagicalYatzyTests.Models.Game
{
    public class DiceResultTests
    {
        [Fact]
        public void XInRowReturnsFirstValueAndAmountOfValuesInRow()
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
                var diceResult = new DieResult() {DiceResults = resultToTest};
                var occurrences = diceResult.CalculateDiceOccurrences();
                foreach (var (diceValue, amountOfDice) in occurrences)
                    Assert.Equal(diceResult.DiceResults.Count(i=>i==diceValue),amountOfDice);
            }
        }
    }
}