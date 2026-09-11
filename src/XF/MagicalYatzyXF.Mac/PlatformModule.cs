using Sanet.MagicalYatzy.Services.Navigation;
using Sanet.MagicalYatzy.Xf;
using Sanet.MagicalYatzy.Xf.Mac.Services;
using SimpleInjector;

namespace Sanet.MagicalYatzy.Xf.Mac
{
    public static class ContainerExtensions
    {
        public static void RegisterModules(this Container container)
        {
            container.RegisterMacModule();
            container.RegisterMainModule();
        }

        public static void RegisterMacModule(this Container container)
        {
            container.RegisterSingleton<IExternalNavigationService, ExternalNavigationService>();
        }
    }
}