using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Xunit;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services;
using NSubstitute;

namespace MagicalYatzyTests.ViewModelTests.ObservableWrappers
{
    public class RollResultViewModelTest
    {
        private readonly RollResult _rollResult = new RollResult(Scores.Ones, Rules.krSimple);
        private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
        private readonly RollResultViewModel _sut;

        public RollResultViewModelTest()
        {
            _sut = new RollResultViewModel(_rollResult, _localizationService);
        }

        [Fact]
        public void HasCorrectRollResultValue()
        {
            Assert.Equal(_rollResult, _sut.RollResult);
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
        public void PossibleValueIsCorrect()
        {
            _rollResult.PossibleValue = 3;
            
            Assert.Equal(3,_sut.PossibleValue);
        }

        [Fact]
        public void HasBonusIsCorrect()
        {
            var kniffelRollResult = new RollResult(Scores.Ones, Rules.krExtended);
            var sut = new RollResultViewModel(kniffelRollResult, _localizationService);
            kniffelRollResult.HasBonus = true;
            
            Assert.True(sut.HasBonus);
        }
        
        [Fact]
        public void ApplyResultChangesValueAndBonus()
        {
            var kniffelRollResult = new RollResult(Scores.Ones,Rules.krExtended);
            var sut = new RollResultViewModel(kniffelRollResult, _localizationService);
            
            var newRollResult = new RollResult(Scores.Ones, Rules.krExtended);
            newRollResult.PossibleValue = 5;
            newRollResult.HasBonus = true;
            
            sut.ApplyResult(newRollResult);
            
            Assert.Equal(5, sut.Value);
            Assert.True(sut.HasValue);
            Assert.True(sut.HasBonus);
        }

        [Fact]
        public void ApplyResultCalledWithNullParameterAppliesSelfResult()
        {
            _rollResult.PossibleValue = 4;

            _sut.ApplyResult();

            Assert.Equal(4, _sut.Value);
            Assert.True(_sut.HasValue);
        }

        [Fact]
        public void ApplyResultNotifiesAboutChangesInValueAndBonus()
        {
            var newRollResult = new RollResult(Scores.Ones, Rules.krSimple);

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

        [Fact]
        public void ShortNameIsCorrect()
        {
            var expectedName = Strings.OnesShort;
            _localizationService.GetLocalizedString("OnesShort").Returns(expectedName);

            Assert.Equal(expectedName, _sut.ShortName);
        }
    }
}