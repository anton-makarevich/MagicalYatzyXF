using Autofac;
using Sanet.MagicalYatzy.XF.iOS.Services;

namespace Sanet.MagicalYatzy.XF.iOS
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
