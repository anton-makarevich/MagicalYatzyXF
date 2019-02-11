using System;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.ViewModels.Base;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers
{
    public class RuleViewModel: BindableBase
    {
        private readonly IRulesService _rulesService;
        private readonly ILocalizationService _localizationService;
        private bool _isSelected;
        
        public event EventHandler RuleSelected;

        public RuleViewModel(Rules rule, 
            IRulesService rulesService,
            ILocalizationService localizationService)
        {
            _rulesService = rulesService;
            _localizationService = localizationService;
            Rule = rule;
        }

        public Rules Rule { get; }

        public string Name => _localizationService.GetLocalizedString(Rule.ToString()).ToUpper();

        public string ShortDescription => 
            _localizationService.GetLocalizedString(Rule.ToString() + "Short");

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ICommand SelectRuleCommand => new SimpleCommand(SelectRule);

        private void SelectRule()
        {
            RuleSelected?.Invoke(this,null);
        }
    }
}