using Android.App;
using Android.Content.PM;
using Android.OS;
using SimpleInjector;

namespace Sanet.MagicalYatzy.XF.Droid
{
    [Activity(Label = "MagicalYatzyXF", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private Container _container;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            _container = new Container();
            _container.RegisterModules();
            App.Container = _container;
            LoadApplication(new App());
        }
    }
}

