using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Xunit;

namespace MagicalYatzyTests.Services
{
    public class GameSettingsServiceTests
    {
        private readonly GameSettingsService _sut = new GameSettingsService();

        [Fact]
        public void StoresDieStyle()
        {
            const DiceStyle style = DiceStyle.Red;
            _sut.DieStyle = style;
            
            Assert.Equal(style,_sut.DieStyle);
        }

        [Fact]
        public void TwoIsDefaultDieAngle()
        {
            Assert.Equal(2,_sut.DieAngle);
        }

        [Fact]
        public void DieAngleCannotBeNegative()
        {
            _sut.DieAngle = -1;
            
            Assert.Equal(0,_sut.DieAngle);
        }

        [Fact]
        public void DieAngleCannotBeHigherThanFive()
        {
            _sut.DieAngle = 6;
            
            Assert.Equal(5, _sut.DieAngle);
        }

        [Fact]
        public void HundredIsDefaultValueForMaxRollLoop()
        {
            Assert.Equal(100,_sut.MaxRollLoop);
        }

        [Fact]
        public void MaxRollLoopCannotBeLessThanTwenty()
        {
            _sut.MaxRollLoop = 19;
            
            Assert.Equal(20, _sut.MaxRollLoop);
        }

        [Fact]
        public void MaxRollLoopCannotBeHigherThan150()
        {
            _sut.MaxRollLoop = 151;
            
            Assert.Equal(150,_sut.MaxRollLoop);
        }
    }
}