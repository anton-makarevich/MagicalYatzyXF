using Sanet.MagicalYatzy.Models.Events;
using Xunit;

namespace MagicalYatzyTests.Models.Events
{
    public class DiceFixedEventArgsTests
    {
        [Fact]
        public void CouldBeCreatedFixDiceValueAndIsFixedParameter()
        {
            const int diceValue = 2;
            const bool isFixed = true;

            var sut = new DiceFixedEventArgs(isFixed, diceValue);
            
            Assert.Equal(isFixed, sut.IsFixed);
            Assert.Equal(diceValue, sut.Value);
        }
    }
}