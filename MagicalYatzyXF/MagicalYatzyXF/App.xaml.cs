using Autofac;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.XF.Helpers;
using Sanet.MagicalYatzy.XF.Services;
using Sanet.MagicalYatzy.XF.Views;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF
{
    public partial class App : Application
    {
        public static IContainer Container { get; private set; }
        public App(IContainer container)
        {
            InitializeComponent();
            Container = container;
            DiceLoaderHelper.PreloadImages();
            MainPage = new MainMenuView()
            {
                ViewModel = new XamarinFormsNavigationService().GetViewModel<MainMenuViewModel>()
            };
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
