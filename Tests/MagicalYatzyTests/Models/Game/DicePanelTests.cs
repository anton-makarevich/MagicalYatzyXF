﻿using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Common;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Xunit;

namespace MagicalYatzyTests.Models.Game
{
    public class DicePanelTests
    {
        private readonly DicePanel _sut;

        private const int TestDiceCount = 6;

        private readonly List<int> _testResults = new() { 2, 4, 3, 3, 1, 5 };
private readonly IGameSettingsService _gameSettingMock = Substitute.For<IGameSettingsService>();
        public DicePanelTests()
        {
            
            _gameSettingMock.MaxRollLoop.Returns(1);
            _gameSettingMock.DieSpeed.Returns(0);
            _sut = new DicePanel(_gameSettingMock)
            {
                DiceCount = TestDiceCount,
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
            // Arrange
            _gameSettingMock.DieSpeed.Returns(10);
            
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
            _sut.RollStarted += (s,e) => { rollStartFired = true; };
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
            _sut.RollEnded += (s,e) => { rollEndFired = true; };
            // Act
            _sut.RollDice(null);

            // Assert
            Assert.True(rollEndFired);
        }

        [Fact]
        public void AllDiceStopWhenRollIsEnded()
        {
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
            // Act
            _sut.RollDice(null);

            // Assert
            foreach (var result in _sut.Result.DiceResults)
                Assert.True(result > 0 && result < 7);
        }

        [Fact]
        public void DiceResultHasValuePassedToRollMethod()
        {
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
            // Arrange
            const int resultToFix = 2;
            
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
            // Arrange
            const int resultToFix = 2;
            
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
            // Arrange
            const int resultToFix = 2;

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

            var chanedEventValidated = false;

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
            // Act
            _sut.RollDice(_testResults);
            var result = _sut.Result.DiceResults.Sum();

            // Assert
            Assert.Equal(result, _sut.Result.Total);
        }

        [Fact]
        public void ReturnsDiceCoordinatesByValue()
        {
            const int diceValueToLookFor = 4;
            _sut.RollDice(_testResults);
            var dice = _sut.Dice.First(f => f.Result == diceValueToLookFor);

            var dicePosition = _sut.GetDicePosition(diceValueToLookFor);

            Assert.NotNull(dicePosition);
            Assert.Equal(new Point(dice.PosX,dice.PosY), dicePosition.Value);
        }

        [Fact]
        public void DefaultValueForPlaySoundIsFalse()
        {
            Assert.False(_sut.PlaySound);
        }

        [Fact]
        public void ChangeDiceDoesNotWorkIfValueIsInvalid()
        {
            // Act
            _sut.RollDice(_testResults);
            _sut.ChangeDice(1, 2);
            _sut.ChangeDice(1, 6);

            // Assert
            Assert.DoesNotContain(6, _sut.Result.DiceResults);
        }
        
        [Fact]
        public void ManualSetModeIsSwitchedOffOnRollDice()
        {
            // Arrange
            _sut.ManualSetMode = true;

            // Act
            _sut.RollDice(_testResults);
            
            // Assert
            Assert.False(_sut.ManualSetMode);
        }

        [Fact]
        public void RollStartedIsNotInvokedIfAllDiceAreFixed()
        {
            // Arrange
            var rollStartedTimes = 0;
            _sut.RollStarted += (sender, args) =>
            {
                rollStartedTimes++;
            };
            _sut.RollDice(_testResults);
            foreach (var result in _sut.Result.DiceResults)  
            {
                _sut.FixDice(result,true);
            }
            
            // Act
            _sut.RollDice(null);
            
            // Assert
            Assert.Equal(1, rollStartedTimes);
        }
        
        [Fact]
        public void FixedDiceValueIsNotOverridenOnNextRoll()
        {
            // Arrange
            _sut.RollDice(_testResults);
            _sut.FixDice(1,true);
            var newResults = new List<int> { 2, 4, 3, 3, 6, 5 };
            
            // Act
            _sut.RollDice(newResults);
            
            // Assert
            Assert.Contains(1, _sut.Result.DiceResults);
        }
        
        [Fact]
        public void ClickOnDiceDoesNotFixItIfLocationIsMissed()
        {
            // Arrange
            const int width = 300;
            const int height = 200;
            _sut.DiceCount = 1;
            _sut.ClickToFix = true;
            _sut.Resize(width, height);
            var diceToSelect = _sut.Dice.First();
            var pointInDice = new Point( diceToSelect.Bounds.Center.X+72, diceToSelect.Bounds.Center.Y+72);

            // Act
            _sut.DieClicked(pointInDice);

            // Assert
            Assert.Equal(0, _sut.FixedDiceCount);
        }
        
        [Fact]
        public void ResizeDoesNotRearrangeDicePositionsIfNewSizeIsTheSame()
        {
            // Arrange
            const int width = 300;
            const int height = 200;
            _sut.DiceCount = 1;
            _sut.Resize(width, height);
            var dicePositionX = _sut.Dice.First().PosX;
            var dicePositionY = _sut.Dice.First().PosY;
           
            // Act
            _sut.Resize(width, height);

            // Assert
            Assert.Equal(dicePositionX, _sut.Dice.First().PosX);
            Assert.Equal(dicePositionY, _sut.Dice.First().PosY);
        }
        
        [Fact]
        public void GetDicePositionReturnsNullIfWrongValueIsPassedAsArgument()
        {
            // Arrange
            _sut.RollDice(_testResults);

            // Act
            var position = _sut.GetDicePosition(6);
            
            // Assert
            Assert.Null(position);
        }
        
        [Fact]
        public void HandleCollisionsDoesNotCrashIfThereIsOneDice()
        {
            // Arrange
            const int width = 300;
            const int height = 200;
            _sut.DiceCount = 1;
            _sut.Resize(width, height);

            // Act
            _sut.RollDice(null);
        }
    }
}
