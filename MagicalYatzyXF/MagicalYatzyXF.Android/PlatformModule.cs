using Sanet.MagicalYatzy.Xf.Droid.Services;
using SimpleInjector;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Navigation;

namespace Sanet.MagicalYatzy.Xf.Droid
{
    public static class ContainerExtensions
    {
        public static void RegisterModules(this Container container)
        {
            container.RegisterAndroidModule();
            container.RegisterMainModule();
        }

        public static void RegisterAndroidModule(this Container container)
        {
            container.RegisterSingleton<IExternalNavigationService, ExternalNavigationService>();
        }
    }
}