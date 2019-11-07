using Sanet.MagicalYatzy.Services.Navigation;
using Sanet.MagicalYatzy.Xf.Gtk.Services;
using SimpleInjector;

namespace Sanet.MagicalYatzy.Xf.Gtk
{
    public static class ContainerExtensions
    {
        public static void RegisterModules(this Container container)
        {
            container.RegisterGtkModule();
            container.RegisterMainModule();
        }

        public static void RegisterGtkModule(this Container container)
        {
            container.RegisterSingleton<IExternalNavigationService, ExternalNavigationService>();
        }
    }
}