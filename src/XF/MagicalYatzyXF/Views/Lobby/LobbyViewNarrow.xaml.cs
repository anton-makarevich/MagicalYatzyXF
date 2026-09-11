using Sanet.MagicalYatzy.Xf.Views.Base;
using Sanet.MagicalYatzy.ViewModels;
using Xamarin.Forms.Xaml;
using Sanet.MagicalYatzy.Xf.Views.Controls.TabControl;
using Sanet.MagicalYatzy.Xf.Views.Fragments;

namespace Sanet.MagicalYatzy.Xf.Views.Lobby
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
            TabBar.TabChildren.Add(new TabItem(ViewModel.PlayersTitle, new PlayersFragment()));
            TabBar.TabChildren.Add(new TabItem(ViewModel.RulesTitle, new RulesFragment()));
        }
    }
}
