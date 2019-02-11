using Sanet.MagicalYatzy.XF.Views.Base;
using Sanet.MagicalYatzy.ViewModels;
using Xamarin.Forms.Xaml;
using Sanet.MagicalYatzy.XF.Controls.TabControl;
using Xamarin.Forms;
using Sanet.MagicalYatzy.XF.Views.Fragments;

namespace Sanet.MagicalYatzy.XF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LobbyViewNarrow : NavigationBackView<LobbyViewModel>
    {
        public LobbyViewNarrow()
        {
            InitializeComponent();
        }

        protected override void OnViewModelSet()
        {
            tabBar.TabChildren.Add(new TabItem(ViewModel.PlayersTitle, new PlayersFragment()));
            tabBar.TabChildren.Add(new TabItem(ViewModel.RulesTitle, new RulesFragment()));
        }
    }
}
