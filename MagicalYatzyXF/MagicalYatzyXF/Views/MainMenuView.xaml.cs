using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.XF.Views.Base;
using Xamarin.Forms.Xaml;

namespace Sanet.MagicalYatzy.XF.Views
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMenuView : DicePanelView<MainMenuViewModel>
    {
        public MainMenuView()
        {
            InitializeComponent();
        }
    }
}
