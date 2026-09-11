using Sanet.MagicalYatzy.Services.Navigation;
using Sanet.MagicalYatzy.Xf.Ios.Services;
using SimpleInjector;

namespace Sanet.MagicalYatzy.Xf.iOS
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
