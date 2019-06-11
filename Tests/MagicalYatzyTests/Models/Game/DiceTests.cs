using System;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Common;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Xunit;

namespace MagicalYatzyTests.Models.Game
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
        public void BothColidingDiceMovingTowardsEachOtherAlongXAxisChangeDirection() 
        {
            // Arrange
            const int thisDirection = 3;
            const int othersDirection = -3;
            _sut.DirectionX = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosX = 40,
                DirectionX = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection * -1, _sut.DirectionX);
            Assert.Equal(othersDirection * -1, otherDice.DirectionX);
        }

        [Fact]
        public void OnlyOneColidingDiceMovingChangesDirectionWhenMovingRight()
        {
            // Arrange
            const int thisDirection = 3;
            const int othersDirection = 2;
            _sut.DirectionX = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosX = 40,
                DirectionX = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection * -1, _sut.DirectionX);
            Assert.Equal(othersDirection, otherDice.DirectionX);
        }

        [Fact]
        public void OnlyOneColidingDiceMovingChangesDirectionWhenMovingLeft()
        {
            // Arrange
            const int thisDirection = -2;
            const int othersDirection = -3;
            _sut.DirectionX = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosX = 40,
                DirectionX = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection, _sut.DirectionX);
            Assert.Equal(othersDirection * -1, otherDice.DirectionX);
        }

        [Fact]
        public void BothColidingDiceMovingTowardsEachOtherAlongYAxisChangeDirection()
        {
            // Arrange
            const int thisDirection = 3;
            const int othersDirection = -3;
            _sut.DirectionY = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosY = 40,
                DirectionY = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection * -1, _sut.DirectionY);
            Assert.Equal(othersDirection * -1, otherDice.DirectionY);
        }

        [Fact]
        public void OnlyOneColidingDiceMovingChangesDirectionWhenMovingDown()
        {
            // Arrange
            const int thisDirection = 3;
            const int othersDirection = 2;
            _sut.DirectionY = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosY = 40,
                DirectionY = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection * -1, _sut.DirectionY);
            Assert.Equal(othersDirection, otherDice.DirectionY);
        }

        [Fact]
        public void OnlyOneColidingDiceMovingChangesDirectionWhenMovingUp()
        {
            // Arrange
            const int thisDirection = -2;
            const int othersDirection = -3;
            _sut.DirectionY = thisDirection;
            var otherDice = new Die(_dicePanelMock, _gameSettingMock)
            {
                PosY = 40,
                DirectionY = -othersDirection
            };

            // Act
            _sut.HandleCollision(otherDice);

            // Assert
            Assert.Equal(thisDirection, _sut.DirectionY);
            Assert.Equal(othersDirection * -1, otherDice.DirectionY);
        }

        [Fact]
        public void FixedDiceChangesOpacity()
        {
            // Arrange
            _sut.IsFixed = true;

            // Act
            _sut.DrawDie();

            // Assert
            Assert.True(_sut.Opacity < 1);
        }

        [Fact]
        public void ImagePathPointsToPngImage()
        {
            // Act
            _sut.DrawDie();

            // Assert
            Assert.EndsWith("png", _sut.ImagePath);
        }

        [Fact]
        public void DrawDiceCalculatesFrameIfDiceIsNotRolling()
        {
            for (var result = 1; result < 7; result++)
            {
                for (var angle = 0; angle < 6; angle++)
                {
                    // Arrange
                    _gameSettingMock.DieAngle.Returns(angle);
                    _sut.Result = result;
                    var expectedFrame = (result - 1) * 6 + angle;

                    // Act
                    _sut.DrawDie();

                    // Assert
                    Assert.EndsWith($"{expectedFrame}.png", _sut.ImagePath);
                }
            }
        }

        [Fact]
        public void ThrowsExceptionWhenResultIsNotValid()
        {
            var exception = Assert.Throws<Exception>(() => { _sut.Result = 8; });
            Assert.StartsWith("Unexpected value", exception.Message);
        }

        [Fact]
        public void InitializePositionReturnsZeroIfPanelIsOfZeroSize()
        {
            _dicePanelMock.Bounds.Returns(new Rectangle(0,0,0,0));
            
            _sut.InitializeLocation();
            
            Assert.Equal(0,_sut.PosX);
            Assert.Equal(0,_sut.PosY);
        }
    }
}
