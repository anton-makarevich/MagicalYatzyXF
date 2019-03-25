using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.Services;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers
{
    public class RollResultViewModel:BindableBase
    {
        private readonly IRollResult _rollResult;
        private readonly ILocalizationService _localizationService;

        public RollResultViewModel(IRollResult rollResult, ILocalizationService localizationService)
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
        public IRollResult RollResult => _rollResult;

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