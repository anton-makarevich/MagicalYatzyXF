using Autofac;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.XF.Services;

namespace Sanet.MagicalYatzy.XF
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register viewmodels for pages
            builder.RegisterType<MainMenuViewModel>();
            builder.RegisterType<LoginViewModel>();

            // Register services
            builder.RegisterType<GameSettingsService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<LocalizationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<WebService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<LegacyWcfClient>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<LocalJsonStorageService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<PlayerService>().AsImplementedInterfaces().SingleInstance();

            // Register misc classes
            builder.RegisterType<DicePanel>().AsImplementedInterfaces();
        }
    }
}
