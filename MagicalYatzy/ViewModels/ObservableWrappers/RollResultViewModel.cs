using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.Services;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers
{
    public class RollResultViewModel:BindableBase
    {
        private readonly RollResult _rollResult;
        private readonly ILocalizationService _localizationService;

        public RollResultViewModel(RollResult rollResult, ILocalizationService localizationService)
        {
            _rollResult = rollResult;
            _localizationService = localizationService;
        }

        public int PossibleValue => _rollResult.PossibleValue;
        public int Value => _rollResult.Value;
        public bool HasValue => _rollResult.HasValue;
        public Scores ScoreType => _rollResult.ScoreType;
        public bool HasBonus => _rollResult.HasBonus;

        public string ShortName => _localizationService.GetLocalizedString(_rollResult.ScoreType+"Short");

        public void ApplyResult(IRollResult result = null)
        {
            if (result == null)
                result = _rollResult;
            _rollResult.Value = result.PossibleValue;
            _rollResult.HasBonus = result.HasBonus;
            
            NotifyPropertyChanged(nameof(Value));
            NotifyPropertyChanged(nameof(HasValue));
            NotifyPropertyChanged(nameof(HasBonus));
        }
    }
}