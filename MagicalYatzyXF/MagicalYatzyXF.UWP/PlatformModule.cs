using Autofac;
using Sanet.MagicalYatzy.XF.UWP.Services;

namespace Sanet.MagicalYatzy.XF.UWP
{
    public class PlatformModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<MainModule>();
            builder.RegisterType<ExternalNavigationService>().AsImplementedInterfaces().SingleInstance();
            base.Load(builder);
        }
    }
}
