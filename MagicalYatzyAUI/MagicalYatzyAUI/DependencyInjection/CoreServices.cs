using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.Services.Stubs;
using Sanet.MagicalYatzy.Dto.ApiConfigs;
using Sanet.MagicalYatzy.Dto.Services;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Api;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.Services.Media;
using Sanet.MagicalYatzy.Services.Navigation;
using Sanet.MagicalYatzy.Services.StorageService;
using Sanet.MagicalYatzy.ViewModels;

namespace Sanet.MagicalYatzy.Avalonia.DependencyInjection;

public static class CoreServices
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IDicePanel,DicePanel>();
        services.AddSingleton<IPlayerService,PlayerService>();
        services.AddSingleton<IExternalNavigationService, ExternalNavigationStub>();
        services.AddSingleton<IGameSettingsService, GameSettingsService>();
        services.AddSingleton<IApiClient, AzureApiClient>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IWebService, WebService>();
        services.AddSingleton<IApiConfig, AzureDevConfig>();
        services.AddSingleton<IRulesService, RulesService>();
        services.AddSingleton<IDiceGenerator, RandomDiceGenerator>();
        services.AddSingleton<ILocalizationService, GlobalizationInvariantLocalizationService>();
        services.AddSingleton<IStorageService, LocalJsonStorageService>();
        services.AddSingleton<ISoundsProvider, SoundsProviderStub>();
    }
    public static void RegisterViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainMenuViewModel, MainMenuViewModel>();
        services.AddTransient<LobbyViewModel, LobbyViewModel>();
        services.AddTransient<GameViewModel, GameViewModel>();
        services.AddTransient<GameResultsViewModel, GameResultsViewModel>();
    }
}