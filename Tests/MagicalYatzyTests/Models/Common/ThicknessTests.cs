using FluentAssertions;
using Sanet.MagicalYatzy.Models.Common;
using Xunit;
namespace MagicalYatzyTests.Models.Common;

public class ThicknessTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateThicknessWithZeroValues()
    {
        // Arrange
        var thickness = new Thickness();

        // Assert
        thickness.Left.Should().Be(0.0);
        thickness.Right.Should().Be(0.0);
        thickness.Top.Should().Be(0.0);
        thickness.Bottom.Should().Be(0.0);
    }

    [Fact]
    public void ConstructorWithFourParameters_ShouldSetValuesCorrectly()
    {
        // Arrange
        const double left = 1.0;
        const double top = 2.0;
        const double right = 3.0;
        const double bottom = 4.0;

        // Act
        var thickness = new Thickness(left, top, right, bottom);

        // Assert
        thickness.Left.Should().Be(left);
        thickness.Right.Should().Be(right);
        thickness.Top.Should().Be(top);
        thickness.Bottom.Should().Be(bottom);
    }

    [Fact]
    public void ConstructorWithOneParameter_ShouldSetAllValuesToTheSame()
    {
        // Arrange
        const double value = 5.0;

        // Act
        var thickness = new Thickness(value);

        // Assert
        thickness.Left.Should().Be(value);
        thickness.Right.Should().Be(value);
        thickness.Top.Should().Be(value);
        thickness.Bottom.Should().Be(value);
    }

    [Fact]
    public void ConstructorWithTwoParameters_ShouldSetHorizontalAndVerticalValuesCorrectly()
    {
        // Arrange
        const double horizontal = 6.0;
        const double vertical = 7.0;

        // Act
        var thickness = new Thickness(horizontal, vertical);

        // Assert
        thickness.Left.Should().Be(horizontal);
        thickness.Right.Should().Be(horizontal);
        thickness.Top.Should().Be(vertical);
        thickness.Bottom.Should().Be(vertical);
    }
}






