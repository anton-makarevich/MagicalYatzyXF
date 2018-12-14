using Sanet.MagicalYatzy.XF.Droid.Services;
using SimpleInjector;
using Sanet.MagicalYatzy.Services;

namespace Sanet.MagicalYatzy.XF.Droid
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