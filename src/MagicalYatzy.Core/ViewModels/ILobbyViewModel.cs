using System.Collections.ObjectModel;
using System.Windows.Input;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.ViewModels;

public interface ILobbyViewModel
{
    ObservableCollection<PlayerViewModel> Players { get; }
    bool CanAddBot { get; }
    bool CanAddHuman { get; }
    string AddBotLabel { get; }
    string AddPlayerLabel { get; }
    string AddBotImage { get; }
    string AddPlayerImage { get; }
    ICommand AddBotCommand { get; }
    ICommand AddHumanCommand { get; }
    ObservableCollection<RuleViewModel> Rules { get; }
    RuleViewModel SelectedRule { get; set; }
    bool IsRulesEditable { get; }
}
