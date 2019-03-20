using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using NSubstitute;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class DicePanelTests
    {
        private readonly DicePanel _sut;

        private const int TestDiceCount = 6;
        private const int TestRollDelay = 6;

        private readonly List<int> _testResults = new List<int> { 2, 4, 3, 3, 1, 5 };

        public DicePanelTests()
        {
            var gameSettingMock = Substitute.For<IGameSettingsService>();
            _sut = new DicePanel(gameSettingMock)
            {
                DiceCount = TestDiceCount,
                RollDelay = TestRollDelay
            };
        }

        [Fact]
        public void DiceIsInitialized()
        {
            // Assert
            Assert.True(_sut.AreDiceGenerated);
        }

        [Fact]
        public void DisposeClearsDice()
        {
            // Act
            _sut.Dispose();

            // Assert
            Assert.False(_sut.AreDiceGenerated);
        }

        [Fact]
        public void ResultIsNotEmpty()
        {
            // Assert
            Assert.Equal(TestDiceCount, _sut.Result.DiceResults.Count);
            Assert.Equal(TestDiceCount, _sut.Result.NumDice);
        }

        [Fact]
        public void AllDiceRollWhenRollIsInvoked()
        {
            // Act
            _sut.RollDice(null);

            // Assert
            Assert.True(_sut.IsRolling);
            foreach (var dice in _sut.Dice)
                Assert.True(dice.IsRolling);
        }

        [Fact]
        public void EventFiresWhenRollIsInvoked()
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
        public void EventFiresWhenRollIsEnded()
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
        public void AllDiceStopWhenRollIsEnded()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(null);

            // Assert
            Assert.True(_sut.AreAllDiceStopped);
            foreach (var dice in _sut.Dice)
                Assert.True(dice.IsNotRolling);
        }

        [Fact]
        public void EveryDiceHasProperResultValue()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(null);

            // Assert
            foreach (var result in _sut.Result.DiceResults)
                Assert.True(result > 0 && result < 7);
        }

        [Fact]
        public void DiceResultHasValuePassedToRollMethod()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(_testResults);

            // Assert
            Assert.Equal(1, _sut.Result.NumDiceOf(1));
            Assert.Equal(1, _sut.Result.NumDiceOf(2));
            Assert.Equal(2, _sut.Result.NumDiceOf(3));
            Assert.Equal(1, _sut.Result.NumDiceOf(4));
            Assert.Equal(1, _sut.Result.NumDiceOf(5));
            Assert.Equal(0, _sut.Result.NumDiceOf(6));
        }

        [Fact]
        public void PanelFixesSpecifiedDice()
        {
            const int resultToFix = 2;

            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(_testResults);
            _sut.FixDice(resultToFix, true);

            // Assert
            Assert.Equal(1, _sut.FixedDiceCount);
            var fixedDice = _sut.Dice.FirstOrDefault(d => d.IsFixed);
            Assert.NotNull(fixedDice);
            Assert.Equal(resultToFix, fixedDice.Result);
        }

        [Fact]
        public void PanelFixesAllDice()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(_testResults);
            foreach(var result in _testResults)
                _sut.FixDice(result, true);

            // Assert
            Assert.True(_sut.AreAllDiceFixed);
        }

        [Fact]
        public void PanelUnfixesSpecifiedDice()
        {
            const int resultToFix = 2;

            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(_testResults);
            _sut.FixDice(resultToFix, true);
            _sut.FixDice(resultToFix, false);

            // Assert
            Assert.Equal(0, _sut.FixedDiceCount);
        }

        [Fact]
        public void PanelUnfixesAllDice()
        {
            const int resultToFix = 2;

            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(_testResults);
            _sut.FixDice(resultToFix, true);
            _sut.UnfixAll();

            // Assert
            Assert.Equal(0, _sut.FixedDiceCount);
        }

        [Fact]
        public void PanelChangesDiceValueWhenRequested()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(_testResults);
            _sut.ChangeDice(1, 6);

            // Assert
            Assert.Equal(0, _sut.Result.NumDiceOf(1));
            Assert.Equal(1, _sut.Result.NumDiceOf(2));
            Assert.Equal(2, _sut.Result.NumDiceOf(3));
            Assert.Equal(1, _sut.Result.NumDiceOf(4));
            Assert.Equal(1, _sut.Result.NumDiceOf(5));
            Assert.Equal(1, _sut.Result.NumDiceOf(6));
        }

        [Fact]
        public void ResizeUpdatesBounds()
        {
            // Arrange
            var width = 300;
            var height = 200;

            // Act
            _sut.Resize(width, height);

            // Assert
            Assert.Equal(0, _sut.Bounds.Left);
            Assert.Equal(0, _sut.Bounds.Top);
            Assert.Equal(width, _sut.Bounds.Width);
            Assert.Equal(height, _sut.Bounds.Height);
        }

        [Fact]
        public void ClickOnDiceFixesItIfAllowed()
        {
            // Arrange
            const int width = 300;
            const int height = 200;
            _sut.ClickToFix = true;
            _sut.Resize(width, height);
            var diceToSelect = _sut.Dice.First();
            var pointInDice = diceToSelect.Bounds.Center;

            // Act
            _sut.DieClicked(pointInDice);

            // Assert
            Assert.Equal(1, _sut.FixedDiceCount);
        }

        [Fact]
        public void ClickOnDiceDoesNotFixItIfNotAllowed()
        {
            // Arrange
            const int width = 300;
            const int height = 200;
            _sut.ClickToFix = false;
            _sut.Resize(width, height);
            var diceToSelect = _sut.Dice.First();
            var pointInDice = diceToSelect.Bounds.Center;

            // Act
            _sut.DieClicked(pointInDice);

            // Assert
            Assert.Equal(0, _sut.FixedDiceCount);
        }

        [Fact]
        public void ClickOnDiceUpdatesItsValueIfManualSetModeIsOn()
        {
            // Arrange
            var width = 300;
            var height = 200;

            var valueToSet = 3;
            var initialValue = 1;

            bool chanedEventValidated = false;

            _sut.ManualSetMode = true;
            _sut.Resize(width, height);
            var diceToSelect = _sut.Dice.First();
            diceToSelect.Result = initialValue;
            var pointInDice = diceToSelect.Bounds.Center;
            _sut.DieManualChangeRequested += (update) => { update(valueToSet); };
            _sut.DieChangedManually += (isFixed, oldValue, newValue) => 
            {
                chanedEventValidated = oldValue == initialValue &&
                    newValue == valueToSet && !isFixed; 
            };

            // Act
            _sut.DieClicked( pointInDice );

            // Assert
            Assert.Equal(0, _sut.FixedDiceCount);
            Assert.Equal(valueToSet, diceToSelect.Result);
            Assert.True(chanedEventValidated);
            Assert.False(_sut.ManualSetMode);
        }

        [Fact]
        public void PanelResultIsTheSumOfAllDice()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(_testResults);
            var result = _sut.Result.DiceResults.Sum();

            // Assert
            Assert.Equal(result, _sut.Result.Total);
        }
    }
}
