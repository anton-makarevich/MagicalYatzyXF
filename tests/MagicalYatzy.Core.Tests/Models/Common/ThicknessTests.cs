using Shouldly;
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
        thickness.Left.ShouldBe(0.0);
        thickness.Right.ShouldBe(0.0);
        thickness.Top.ShouldBe(0.0);
        thickness.Bottom.ShouldBe(0.0);
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
        thickness.Left.ShouldBe(left);
        thickness.Right.ShouldBe(right);
        thickness.Top.ShouldBe(top);
        thickness.Bottom.ShouldBe(bottom);
    }

    [Fact]
    public void ConstructorWithOneParameter_ShouldSetAllValuesToTheSame()
    {
        // Arrange
        const double value = 5.0;

        // Act
        var thickness = new Thickness(value);

        // Assert
        thickness.Left.ShouldBe(value);
        thickness.Right.ShouldBe(value);
        thickness.Top.ShouldBe(value);
        thickness.Bottom.ShouldBe(value);
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
        thickness.Left.ShouldBe(horizontal);
        thickness.Right.ShouldBe(horizontal);
        thickness.Top.ShouldBe(vertical);
        thickness.Bottom.ShouldBe(vertical);
    }
}






