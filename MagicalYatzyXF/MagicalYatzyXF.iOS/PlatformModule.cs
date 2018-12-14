using Sanet.MagicalYatzy.XF.iOS.Services;
using Sanet.MagicalYatzy.Services;
using SimpleInjector;

namespace Sanet.MagicalYatzy.XF.iOS
{
    public static class ContainerExtensions
    {
        public static void RegisterModules(this Container container)
        {
            container.RegisterIosModule();
            container.RegisterMainModule();
        }

        public static void RegisterIosModule(this Container container)
        {
            container.RegisterSingleton<IExternalNavigationService, ExternalNavigationService>();
        }
    }
}
