using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.XF.Services;
using SimpleInjector;

namespace Sanet.MagicalYatzy.XF
{
    public static class ContainerExtensions
    {
        public static void RegisterMainModule(this Container container)
        {
            // Register viewmodels for pages
            container.Register<MainMenuViewModel>();
            container.Register<LoginViewModel>();

            // Register services
            container.RegisterSingleton<IGameSettingsService, GameSettingsService>();
            container.RegisterSingleton<ILocalizationService, LocalizationService>();
            container.RegisterSingleton<IWebService, WebService>();
            container.RegisterSingleton<IApiClient, LegacyWcfClient>();
            container.RegisterSingleton<IStorageService, LocalJsonStorageService>();
            container.RegisterSingleton<IPlayerService, PlayerService>();

            // Register misc classes
            container.Register<IDicePanel, DicePanel>();
        }
    }
}
