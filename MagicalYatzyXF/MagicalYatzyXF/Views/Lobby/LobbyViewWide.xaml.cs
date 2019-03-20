using Sanet.MagicalYatzy.XF.Views.Base;
using Sanet.MagicalYatzy.ViewModels;
using Xamarin.Forms.Xaml;

namespace Sanet.MagicalYatzy.XF.Views.Lobby
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
