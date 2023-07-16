using FluentAssertions;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Xunit;

namespace MagicalYatzyTests.Services;

public class GameSettingsServiceTests
{
    private readonly GameSettingsService _sut = new();

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
        
    [Fact]
    public void DieSpeed_ShouldSetProvidedValue_WhenWithinAllowedRange()
    {
        // Arrange
        const int expectedDieSpeed = 30;

        // Act
        _sut.DieSpeed = expectedDieSpeed;

        // Assert
        _sut.DieSpeed.Should().Be(expectedDieSpeed);
    }

    [Theory]
    [InlineData(10, 15)]  // Less than allowed range, set to 15
    [InlineData(75, 70)]  // Greater than allowed range, set to 70
    public void DieSpeed_ShouldSetThresholdValue_WhenOutsideAllowedRange(int inputDieSpeed, int expectedDieSpeed)
    {
        // Act
        _sut.DieSpeed = inputDieSpeed;

        // Assert
        _sut.DieSpeed.Should().Be(expectedDieSpeed);
    }

    [Fact]
    public void IsSoundEnabled_ShouldSetProvidedValue_WhenWithinAllowedRange()
    {
        _sut.IsSoundEnabled = true;
            
        _sut.IsSoundEnabled.Should().BeTrue();
    }
}