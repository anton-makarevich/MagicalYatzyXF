using System.Linq;
using Avalonia.Controls;
using Sanet.MagicalYatzy.Avalonia.Views.Base;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.ViewModels;

namespace Sanet.MagicalYatzy.Avalonia.Views;

public partial class MainMenuView : DicePanelView<MainMenuViewModel>
{
    public MainMenuView()
    {
        InitializeComponent();
    }

    private void MainMenuList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        e.AddedItems.OfType<MainMenuAction>().FirstOrDefault()?.MenuAction.Execute(null);
    }
}