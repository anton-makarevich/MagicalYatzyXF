using FluentAssertions;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Xunit;

namespace MagicalYatzyTests.Models.Game.DiceGenerator;

public class RandomValueGeneratorTests
{
    private readonly RandomValueGenerator _sut = new();

    [Fact]
    public void GeneratesValueInARange()
    {
        const int min = 1;
        const int max = 6;
        var value = _sut.Next(min, max);
        value.Should().BeGreaterOrEqualTo(min);
        value.Should().BeLessThan(max);
    }
}