using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.Xf.Views.Base;
using Xamarin.Forms.Xaml;

namespace Sanet.MagicalYatzy.Xf.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginView : BaseView<LoginViewModel>
    {
        public LoginView()
        {
            InitializeComponent();
        }
    }
}
