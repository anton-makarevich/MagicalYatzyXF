using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests.ObservableWrappers
{
    public class RollResultViewModelTest
    {
        private readonly RollResult _rollResult = new RollResult(Scores.Ones);
        private readonly RollResultViewModel _sut;

        public RollResultViewModelTest()
        {
            _sut = new RollResultViewModel(_rollResult);
        }

        [Fact]
        public void ValueIsEqualToValueIfModelHasValue()
        {
            _rollResult.Value = 5;
            
            Assert.Equal(5,_sut.Value);
            Assert.True(_sut.HasValue);
        }
        
        [Fact]
        public void ValueIsEqualToZeroValueIfModelDoesNotHaveValue()
        {
            _rollResult.PossibleValue = 4;
            
            Assert.Equal(0,_sut.Value);
            Assert.False(_sut.HasValue);
        }

        [Fact]
        public void ScoreTypeIsCorrect()
        {
            Assert.Equal(Scores.Ones, _sut.ScoreType);
        }

        [Fact]
        public void HasBonusIsCorrect()
        {
            var kniffelRollResult = new RollResult(Scores.Kniffel);
            var sut = new RollResultViewModel(kniffelRollResult);
            kniffelRollResult.HasBonus = true;
            
            Assert.True(sut.HasBonus);
        }
        
        [Fact]
        public void ApplyResultChangesValueAndBonus()
        {
            var kniffelRollResult = new RollResult(Scores.Kniffel);
            var sut = new RollResultViewModel(kniffelRollResult);
            
            var newRollResult = new RollResult(Scores.Kniffel);
            newRollResult.PossibleValue = 50;
            newRollResult.HasBonus = true;
            
            sut.ApplyResult(newRollResult);
            
            Assert.Equal(50, sut.Value);
            Assert.True(sut.HasValue);
            Assert.True(sut.HasBonus);
        }
        
        [Fact]
        public void ApplyResultNotifiesAboutChangesInValueAndBonus()
        {
            var newRollResult = new RollResult(Scores.Ones);

            var valueChangedTimes = 0;
            var hasValueChangedTimes = 0;
            var hasBonusChangedTimes = 0;

            _sut.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(_sut.Value):
                        valueChangedTimes++;
                        break;
                    case nameof(_sut.HasValue):
                        hasValueChangedTimes++;
                        break;
                    case nameof(_sut.HasBonus):
                        hasBonusChangedTimes++;
                        break;
                }
            }; 
            
            _sut.ApplyResult(newRollResult);
            
            Assert.Equal(1, valueChangedTimes);
            Assert.Equal(1,hasValueChangedTimes);
            Assert.Equal(1, hasBonusChangedTimes);
        }
    }
}