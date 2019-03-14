using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.XF.Views.Base;
using Xamarin.Forms.Xaml;

namespace Sanet.MagicalYatzy.XF.Views.Game
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GameViewWide : BaseView<GameViewModel>
    {
        public GameViewWide()
        {
            InitializeComponent();
        }
    }
}
