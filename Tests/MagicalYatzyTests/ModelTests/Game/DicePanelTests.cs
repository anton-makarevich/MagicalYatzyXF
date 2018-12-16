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
        private readonly IGameSettingsService _gameSettingMock;

        private const int TestDiceCount = 6;
        private const int TestRollDelay = 6;

        private readonly List<int> TestResults = new List<int> { 2, 4, 3, 3, 1, 5 };

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
        public void DiceShouldBeInitialized()
        {
            // Assert
            Assert.True(_sut.AreDiceGenerated);
        }

        [Fact]
        public void DisposeShouldClearDice()
        {
            // Act
            _sut.Dispose();

            // Assert
            Assert.False(_sut.AreDiceGenerated);
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
            Assert.True(_sut.IsRolling);
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
            Assert.True(_sut.AreAllDiceStopped);
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

        [Fact]
        public void DiceResultShouldHaveValuePassedToRollMethod()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(TestResults);

            // Assert
            Assert.Equal(1, _sut.Result.NumDiceOf(1));
            Assert.Equal(1, _sut.Result.NumDiceOf(2));
            Assert.Equal(2, _sut.Result.NumDiceOf(3));
            Assert.Equal(1, _sut.Result.NumDiceOf(4));
            Assert.Equal(1, _sut.Result.NumDiceOf(5));
            Assert.Equal(0, _sut.Result.NumDiceOf(6));
        }

        [Fact]
        public void PanelShouldFixSpecifiedDice()
        {
            var resultToFix = 2;

            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(TestResults);
            _sut.FixDice(resultToFix, true);

            // Assert
            Assert.Equal(1, _sut.FixedDiceCount);
            var fixedDice = _sut.Dice.FirstOrDefault(d => d.IsFixed);
            Assert.NotNull(fixedDice);
            Assert.Equal(resultToFix, fixedDice.Result);
        }

        [Fact]
        public void PanelShouldFixAllDice()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(TestResults);
            foreach(var result in TestResults)
                _sut.FixDice(result, true);

            // Assert
            Assert.True(_sut.AreAllDiceFixed);
        }

        [Fact]
        public void PanelShouldUnfixSpecifiedDice()
        {
            var resultToFix = 2;

            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(TestResults);
            _sut.FixDice(resultToFix, true);
            _sut.FixDice(resultToFix, false);

            // Assert
            Assert.Equal(0, _sut.FixedDiceCount);
        }

        [Fact]
        public void PanelShouldUnfixAllDice()
        {
            var resultToFix = 2;

            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(TestResults);
            _sut.FixDice(resultToFix, true);
            _sut.UnfixAll();

            // Assert
            Assert.Equal(0, _sut.FixedDiceCount);
        }

        [Fact]
        public void PanelShouldChangeDiceValueWhenRequested()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(TestResults);
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
        public void ResizeShouldUpdateBounds()
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
        public void ClickOnDiceShouldFixItIfAllowed()
        {
            // Arrange
            var width = 300;
            var height = 200;
            _sut.ClickToFix = true;
            _sut.Resize(width, height);
            var diceToSelect = _sut.Dice.First();
            var pontInDice = diceToSelect.Bounds.Center;

            // Act
            _sut.DieClicked(pontInDice);

            // Assert
            Assert.Equal(1, _sut.FixedDiceCount);
            Assert.Equal(diceToSelect, _sut._lastClickedDie);
        }

        [Fact]
        public void ClickOnDiceShoulNotFixItINotAllowed()
        {
            // Arrange
            var width = 300;
            var height = 200;
            _sut.ClickToFix = false;
            _sut.Resize(width, height);
            var diceToSelect = _sut.Dice.First();
            var pontInDice = diceToSelect.Bounds.Center;

            // Act
            _sut.DieClicked(pontInDice);

            // Assert
            Assert.Equal(0, _sut.FixedDiceCount);
        }

        [Fact]
        public void ClickOnDiceShoulUpdateItsValueIfManualSetModeIsOn()
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
            var pontInDice = diceToSelect.Bounds.Center;
            _sut.DieManualChangeRequested += (update) => { update(valueToSet); };
            _sut.DieChangedManually += (isFixed, oldValue, newValue) => 
            {
                chanedEventValidated = oldValue == initialValue &&
                    newValue == valueToSet && !isFixed; 
            };

            // Act
            _sut.DieClicked( pontInDice );

            // Assert
            Assert.Equal(0, _sut.FixedDiceCount);
            Assert.Equal(valueToSet, diceToSelect.Result);
            Assert.True(chanedEventValidated);
            Assert.False(_sut.ManualSetMode);
        }

        [Fact]
        public void PanelResultShouldBeTheSumOfAllDice()
        {
            // Arrange
            _sut.RollDelay = 0;

            // Act
            _sut.RollDice(TestResults);
            var result = _sut.Result.DiceResults.Sum();

            // Assert
            Assert.Equal(result, _sut.Result.Total);
        }
    }
}
