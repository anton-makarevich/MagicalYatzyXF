using Autofac;
using Sanet.MagicalYatzy.XF.Droid.Services;

namespace Sanet.MagicalYatzy.XF.Droid
{
    public class PlatformModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExternalNavigationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterModule<MainModule>();

            base.Load(builder);
        }
    }
}