using Sanet.MagicalYatzy.Xf.Views.Base;
using Sanet.MagicalYatzy.ViewModels;
using Xamarin.Forms.Xaml;

namespace Sanet.MagicalYatzy.Xf.Views.Lobby
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LobbyViewWide : NavigationBackView<LobbyViewModel>
    {
        public LobbyViewWide()
        {
            InitializeComponent();
        }
    }
}
