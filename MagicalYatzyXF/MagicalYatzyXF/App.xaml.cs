using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.XF.Helpers;
using Sanet.MagicalYatzy.XF.Services;
using Sanet.MagicalYatzy.XF.Views;
using SimpleInjector;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF
{
    public partial class App : Application
    {
        public static Container Container { get; set; }
        public App()
        {
            InitializeComponent();
            DiceLoaderHelper.PreloadImages();
            MainPage = new NavigationPage( new MainMenuView()
            {
                ViewModel = new XamarinFormsNavigationService().GetViewModel<MainMenuViewModel>()
            });
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
