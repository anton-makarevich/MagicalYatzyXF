using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests.ObservableWrappers
{
    public class RollResultViewModelTest
    {
        [Fact]
        public void AcceptsRollResultInConstructor()
        {
            var rollResult = new RollResult(Scores.Ones);
            var sut = new RollResultViewModel(rollResult);

            Assert.Equal(rollResult, sut.RollResult);
        }
    }
}