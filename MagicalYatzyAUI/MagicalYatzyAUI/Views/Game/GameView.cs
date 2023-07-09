using Avalonia.Interactivity;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MVVM.Views.Avalonia;
using Sanet.MagicalYatzy.Avalonia.Controls.Game;

namespace Sanet.MagicalYatzy.Avalonia.Views.Game;

public abstract class GameView: BaseView<GameViewModel>
{
    protected DicePanelControl? DicePanelView;
    protected override void OnLoaded(RoutedEventArgs e)
    {
        InitDicePanel();
        base.OnLoaded(e);
    }

    protected virtual void InitDicePanel()
    {
        if (DicePanelView != null)
            return;

        if (ViewModel == null) return;
        DicePanelView = new DicePanelControl(true)
        {
            DicePanel = ViewModel.DicePanel
        };

        ViewModel.DicePanel.DiceCount = 5;
        ViewModel.DicePanel.RollDelay = 30;
    }
}