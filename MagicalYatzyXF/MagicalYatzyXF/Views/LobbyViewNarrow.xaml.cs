using Sanet.MagicalYatzy.XF.Views.Base;
using Sanet.MagicalYatzy.ViewModels;
using Xamarin.Forms.Xaml;
using Sanet.MagicalYatzy.XF.Controls.TabControl;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LobbyViewNarrow : NavigationBackView<LobbyViewModel>
    {
        public LobbyViewNarrow()
        {
            InitializeComponent();
            tabBar.TabChildren.Add(new TabItem("Players", new StackLayout()));
            tabBar.TabChildren.Add(new TabItem("Rules", new StackLayout()));

        }
    }
}
