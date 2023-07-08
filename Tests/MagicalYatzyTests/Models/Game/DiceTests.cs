using System;
using FluentAssertions;
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

        private readonly Rectangle _dicePanelBounds = new(0, 0, 200, 200);

        public DiceTests()
        {
            _gameSettingMock = Substitute.For<IGameSettingsService>();
            _dicePanelMock = Substitute.For<IDicePanel>();
            _dicePanelMock.Bounds.Returns((_) => _dicePanelBounds);
            _dicePanelMock.SaveMargins.Returns(new Thickness());
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
        public void RollingDiceIsOpaque()
        {
            // Arrange
            _sut.Status = DieStatus.Rolling;

            // Act
            _sut.DrawDie();

            // Assert
            Assert.Equal(1,_sut.Opacity);
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
            
            _sut.InitializePosition();
            
            Assert.Equal(0,_sut.PosX);
            Assert.Equal(0,_sut.PosY);
        }

        [Fact]
        public void DicePositionIsAlwaysWithinDicePanelBounds()
        {
            _dicePanelMock.Bounds.Returns(_dicePanelBounds);
            
            _sut.PosX = -3;
            _sut.PosY = -3;
            
            Assert.Equal(0,_sut.PosX);
            Assert.Equal(0,_sut.PosY);
            
            _sut.PosX = 190;
            _sut.PosY = 190;
            
            Assert.Equal(_dicePanelBounds.Width-_sut.Bounds.Width,_sut.PosX);
            Assert.Equal(_dicePanelBounds.Height-_sut.Bounds.Height,_sut.PosY);
        }

        [Fact]
        public void InitializeRollHandlesInvalidVales()
        {
            _sut.InitializeRoll(-3);
            
            Assert.True(_sut.Result>0 && _sut.Result<7);
        }

        [Fact]
        public void InitializeRollOnFixedDiceSetsItsStatusToStoppedAndDoesntChangeValue()
        {
            _sut.Result = 4;
            _sut.IsFixed = true;
            
            _sut.InitializeRoll(3);

            Assert.Equal(DieStatus.Stopped, _sut.Status);
            Assert.Equal(4,_sut.Result);
        }

        [Fact]
        public void RegularMovementDoesNotChangeDiceDirection()
        {
            _sut.InitializePosition();
            _sut.Status = DieStatus.Rolling;
            _sut.DirectionX = 5;
            _sut.DirectionY = 5;
            
            _sut.UpdateDiePosition();
            
            Assert.Equal(5,_sut.DirectionX);
            Assert.Equal(5,_sut.DirectionY);
        }
        
        [Fact]
        public void InitializePosition_WithinSaveMargins_ShouldSetPositionOutsideMargins()
        {
            // Arrange
            _dicePanelMock.SaveMargins.Returns(new Thickness(10, 20, 30, 40));
            
            // Act
            _sut.InitializePosition();
    
            // Assert
            _sut.PosX.Should().BeGreaterOrEqualTo((int)_dicePanelMock.SaveMargins.Left + 1);
            _sut.PosX.Should().BeLessOrEqualTo((int)_dicePanelMock.Bounds.Width - 72 - (int)_dicePanelMock.SaveMargins.Right);

            _sut.PosY.Should().BeGreaterOrEqualTo((int)_dicePanelMock.SaveMargins.Top + 1);
            _sut.PosY.Should().BeLessOrEqualTo((int)_dicePanelMock.Bounds.Height - 72 - (int)_dicePanelMock.SaveMargins.Bottom);
        }

        [Fact]
        public void UpdateDiePosition_DicePositionWithinSaveMargins_ShouldNotSetStatusToLanding()
        {
            // Arrange
            _dicePanelMock.SaveMargins.Returns(new Thickness(10, 20, 30, 40));
            
            _sut.PosX = 5;
            _sut.PosY = 10;
            _sut.Status = DieStatus.Rolling;

            // Act
            _sut.UpdateDiePosition();

            // Assert
            _sut.Status.Should().NotBe(DieStatus.Landing);
        }
    }
}
