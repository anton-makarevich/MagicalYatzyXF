using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

public class MainMenuActionViewModel : BaseViewModel
{
    private readonly MainMenuAction _action;
    private readonly ILocalizationService _localizationService;

    public MainMenuActionViewModel(MainMenuAction action, ILocalizationService localizationService)
    {
        _action = action;
        _localizationService = localizationService;
    } 
    
    #region Properties

    public ICommand MenuAction => _action.MenuAction;
        
    public string Label=> _localizationService.GetLocalizedString(_action.Label);

    public string Description => _localizationService.GetLocalizedString(_action.Description);
        
    public string Image => _action.Image;
    #endregion
}