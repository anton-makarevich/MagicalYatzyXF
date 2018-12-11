using Android.App;
using Android.Content.PM;
using Android.OS;
using Autofac;

namespace Sanet.MagicalYatzy.XF.Droid
{
    [Activity(Label = "MagicalYatzyXF", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private IContainer _container;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            var builder = new ContainerBuilder();
            builder.RegisterModule<PlatformModule>();
            _container = builder.Build();
            LoadApplication(new App(_container));
        }
    }
}

