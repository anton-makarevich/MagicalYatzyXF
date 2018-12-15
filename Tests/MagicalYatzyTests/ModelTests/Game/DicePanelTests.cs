using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using NSubstitute;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class DicePanelTests
    {
        private readonly DicePanel _sut;
        private readonly IGameSettingsService _gameSettingMock;

        private const int TestDiceCount = 6;
        private const int TestRollDelay = 6;


        public DicePanelTests()
        {
            _gameSettingMock = Substitute.For<IGameSettingsService>();
            _sut = new DicePanel(_gameSettingMock)
            {
                DiceCount = TestDiceCount,
                RollDelay = TestRollDelay
            };
        }

        [Fact]
        public void ResultShouldNotBeEmpty()
        {
            // Assert
            Assert.Equal(TestDiceCount, _sut.Result.DiceResults.Count);
            Assert.Equal(TestDiceCount, _sut.Result.NumDice);
        }

        [Fact]
        public void AllDiceShouldRollWhenRollIsInvoked()
        {
            // Act
            _sut.RollDice(null);

            // Assert
            foreach (var dice in _sut.Dice)
                Assert.True(dice.IsRolling);
        }

        [Fact]
        public void EventShouldFireWhenRollIsInvoked()
        {
            // Arrange
            var rollStartFired = false;
            _sut.RollStarted += () => { rollStartFired = true; };
            // Act
            _sut.RollDice(null);

            // Assert
            Assert.True(rollStartFired);
        }

        [Fact]
        public void EventShouldFireWhenRollIsEnded()
        {
            // Arrange
            var rollEndFired = false;
            _sut.RollDelay = 0;
            _sut.RollEnded += () => { rollEndFired = true; };
            // Act
            _sut.RollDice(null);

            // Assert
            Assert.True(rollEndFired);
        }

        [Fact]
        public void AllDiceShouldStopWhenRollIsEnded()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(null);

            // Assert
            Assert.True(_sut.AllDiceStopped);
            foreach (var dice in _sut.Dice)
                Assert.True(dice.IsNotRolling);
        }

        [Fact]
        public void EveryDiceShouldHaveProperResultValue()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(null);

            // Assert
            foreach (var result in _sut.Result.DiceResults)
                Assert.True(result > 0 && result < 7);
        }
    }
}
