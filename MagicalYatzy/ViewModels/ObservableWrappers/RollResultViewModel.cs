using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers
{
    public class RollResultViewModel:BindableBase
    {
        private readonly RollResult _rollResult;

        public RollResultViewModel(RollResult rollResult)
        {
            _rollResult = rollResult;
        }

        public int Value => _rollResult.Value;
        public bool HasValue => _rollResult.HasValue;
        public Scores ScoreType => _rollResult.ScoreType;
        public bool HasBonus => _rollResult.HasBonus;

        public void ApplyResult(IRollResult result)
        {
            _rollResult.Value = result.PossibleValue;
            _rollResult.HasBonus = result.HasBonus;
            
            NotifyPropertyChanged(nameof(Value));
            NotifyPropertyChanged(nameof(HasValue));
            NotifyPropertyChanged(nameof(HasBonus));
        }
    }
}