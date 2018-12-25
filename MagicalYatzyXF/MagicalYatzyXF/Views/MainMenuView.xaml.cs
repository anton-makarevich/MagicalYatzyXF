using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.XF.Views.Base;
using Xamarin.Forms.Xaml;
using Sanet.MagicalYatzy.Models;

namespace Sanet.MagicalYatzy.XF.Views
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMenuView : DicePanelView<MainMenuViewModel>
    {
        public MainMenuView()
        {
            InitializeComponent();
        }

        void MainMenuItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            if (e?.SelectedItem is MainMenuAction action)
                action.MenuAction.Execute(null);

            MainMenuList.SelectedItem = null;
        }
    }
}
