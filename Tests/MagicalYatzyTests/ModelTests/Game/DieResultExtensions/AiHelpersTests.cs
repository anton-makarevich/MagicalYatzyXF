using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DieResultExtensions;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game.DieResultExtensions
{
    public class AiHelpersTests
    {
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
        public void XInRowReturnsFirstValueAndAmountOfValuesInRow()
        {
            var sut = new DieResult {DiceResults = new List<int> {3, 2, 4, 2, 4}};
            var (firstValue, numberOfValuesInRow) = sut.XInRow();
            Assert.Equal(2,firstValue);
            Assert.Equal(3,numberOfValuesInRow);
            
            sut = new DieResult {DiceResults = new List<int> {3, 2, 4, 2, 1}};
            (firstValue, numberOfValuesInRow) = sut.XInRow();
            Assert.Equal(1,firstValue);
            Assert.Equal(4,numberOfValuesInRow);
            
            sut = new DieResult {DiceResults = new List<int> {3, 5, 4, 2, 6}};
            (firstValue, numberOfValuesInRow) = sut.XInRow();
            Assert.Equal(2,firstValue);
            Assert.Equal(5,numberOfValuesInRow);
        }
    }
}