using NSubstitute;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Events
{
    public class RollResultsEventArgsTests
    {
        [Fact]
        public void CouldBeCreatedWithPlayerValueScoreTypeAndBonusFlag()
        {
            const int diceValue = 2;
            const bool hasBonus = true;
            const Scores scoreType = Scores.Ones;
            var player = Substitute.For<IPlayer>();

            var sut = new RollResultEventArgs(player,diceValue,scoreType,hasBonus);
            
            Assert.Equal(hasBonus, sut.HasBonus);
            Assert.Equal(diceValue, sut.Value);
            Assert.Equal(scoreType,sut.ScoreType);
        }
    }
}