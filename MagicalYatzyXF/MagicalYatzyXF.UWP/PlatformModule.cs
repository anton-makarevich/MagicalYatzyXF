using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.XF.UWP.Services;
using SimpleInjector;

namespace Sanet.MagicalYatzy.XF.UWP
{
    public static class ContainerExtensions
    {
        public static void RegisterModules(this Container container)
        {
            container.RegisterUwpModule();
            container.RegisterMainModule();
        }

        public static void RegisterUwpModule(this Container container)
        {
            container.RegisterSingleton<IExternalNavigationService, ExternalNavigationService>();
        }
    }
}
