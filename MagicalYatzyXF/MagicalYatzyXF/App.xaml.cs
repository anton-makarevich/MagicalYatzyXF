using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.Xf.Helpers;
using Sanet.MagicalYatzy.Xf.Services;
using Sanet.MagicalYatzy.Xf.Views;
using SimpleInjector;
using Xamarin.Forms;
using Sanet.MagicalYatzy.Xf.Models;

namespace Sanet.MagicalYatzy.Xf
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

        public static FormFactor FormFactor => 
            (Device.Idiom == TargetIdiom.Phone) ? FormFactor.Narrow : FormFactor.Wide;

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
