using Sanet.MagicalYatzy.Common.Services;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Api;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Media;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.Xf.Services;
using Sanet.MagicalYatzy.Xf.Services.Media;
using SimpleInjector;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Services.StorageService;

namespace Sanet.MagicalYatzy.Xf
{
    public static class ContainerExtensions
    {
        public static void RegisterMainModule(this Container container)
        {
            // Register app start viewmodel
            container.Register<MainMenuViewModel>();

            // Register services
            container.RegisterSingleton<IDiceGenerator, RandomDiceGenerator>();
            container.RegisterSingleton<IGameSettingsService, GameSettingsService>();
            container.RegisterSingleton<ILocalizationService, LocalizationService>();
            container.RegisterSingleton<IWebService, WebService>();
            container.RegisterSingleton<IApiClient, LegacyWcfClient>();
            container.RegisterSingleton<IStorageService, LocalJsonStorageService>();
            container.RegisterSingleton<IGameService, GameService>();
            container.RegisterSingleton<IPlayerService, PlayerService>();
            container.RegisterSingleton<IRulesService, RulesService>();
            container.RegisterSingleton<ISoundsProvider, EmptySoundsProvider>();

            // Register misc classes
            container.Register<IDicePanel, DicePanel>();
        }
    }
}
