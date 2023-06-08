using System.Threading.Tasks;
using Avalonia.Controls;
using Sanet.MagicalYatzy.Avalonia.Extensions;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MVVM.Views.Avalonia;

namespace Sanet.MagicalYatzy.Avalonia.Views;

public partial class MainMenuView : BaseView<MainMenuViewModel>
{
    public MainMenuView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        Test();
    }

    private async Task Test()
    {
        for (var i = 0; i < 12; i++)
        {
            await Rectangle1.AnimateClick();
            await Task.Delay(5000);
        }
    }
}