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

        public void ApplyResult((int, bool)? result = null)
        {
            if (result == null)
                result = (_rollResult.PossibleValue, _rollResult.HasBonus);
            _rollResult.Value = result.Value.Item1;
            _rollResult.HasBonus = result.Value.Item2;
            
            NotifyPropertyChanged(nameof(Value));
            NotifyPropertyChanged(nameof(HasValue));
            NotifyPropertyChanged(nameof(HasBonus));
        }
    }
}