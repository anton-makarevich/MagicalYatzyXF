using System;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers
{
    public class RuleViewModel: BindableBase
    {
        private readonly ILocalizationService _localizationService;
        private bool _isSelected;
        
        public event EventHandler RuleSelected;

        public RuleViewModel(Rules rule,
            ILocalizationService localizationService)
        {
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
            if (!IsSelected)
                RuleSelected?.Invoke(this,null);
        }
    }
}