using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Xunit;
using Sanet.MagicalYatzy.Models.Common;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class DiceTests
    {
        private readonly Die _sut;
        private readonly IGameSettingsService _gameSettingMock;
        private readonly IDicePanel _dicePanelMock;

        private readonly Rectangle _dicePanelBounds = new Rectangle(0, 0, 200, 200);

        public DiceTests()
        {
            _gameSettingMock = Substitute.For<IGameSettingsService>();
            _dicePanelMock = Substitute.For<IDicePanel>();
            _dicePanelMock.Bounds.Returns((arg) => _dicePanelBounds);
            _sut = new Die(_dicePanelMock, _gameSettingMock);
        }

        [Fact]
        public void BothColidingDiceMovingTowardsEachOtherAlongXAxisShouldChangeDirection() 
        {
            // Arrange
            var thisDirection = 3;
            var othersDirection = -3;
            _sut._directionX = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosX = 40,
                _directionX = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection * -1, _sut._directionX);
            Assert.Equal(othersDirection * -1, otherDice._directionX);
        }

        [Fact]
        public void OnlyOneColidingDiceMovingShouldChangeDirectionWhenMovingRight()
        {
            // Arrange
            var thisDirection = 3;
            var othersDirection = 2;
            _sut._directionX = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosX = 40,
                _directionX = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection * -1, _sut._directionX);
            Assert.Equal(othersDirection, otherDice._directionX);
        }

        [Fact]
        public void OnlyOneColidingDiceMovingShouldChangeDirectionWhenMovingLeft()
        {
            // Arrange
            var thisDirection = -2;
            var othersDirection = -3;
            _sut._directionX = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosX = 40,
                _directionX = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection, _sut._directionX);
            Assert.Equal(othersDirection * -1, otherDice._directionX);
        }

        [Fact]
        public void BothColidingDiceMovingTowardsEachOtherAlongYAxisShouldChangeDirection()
        {
            // Arrange
            var thisDirection = 3;
            var othersDirection = -3;
            _sut._directionY = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosY = 40,
                _directionY = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection * -1, _sut._directionY);
            Assert.Equal(othersDirection * -1, otherDice._directionY);
        }

        [Fact]
        public void OnlyOneColidingDiceMovingShouldChangeDirectionWhenMovingDown()
        {
            // Arrange
            var thisDirection = 3;
            var othersDirection = 2;
            _sut._directionY = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosY = 40,
                _directionY = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection * -1, _sut._directionY);
            Assert.Equal(othersDirection, otherDice._directionY);
        }

        [Fact]
        public void OnlyOneColidingDiceMovingShouldChangeDirectionWhenMovingUp()
        {
            // Arrange
            var thisDirection = -2;
            var othersDirection = -3;
            _sut._directionY = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosY = 40,
                _directionY = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection, _sut._directionY);
            Assert.Equal(othersDirection * -1, otherDice._directionY);
        }
    }
}
