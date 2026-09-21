using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.Localization;
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
        
    public string Label=> _localizationService.GetString(_action.Label);

    public string Description => _localizationService.GetString(_action.Description);
        
    public string Image => _action.Image;
    #endregion
}