using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.Services.Navigation;
using Sanet.MagicalYatzy.Dto.ApiConfigs;
using Sanet.MagicalYatzy.Dto.Services;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Api;
using Sanet.MagicalYatzy.Services.Game;
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
        services.AddSingleton<IStorageService, LocalJsonStorageService>();

    }
    public static void RegisterViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainMenuViewModel, MainMenuViewModel>();
    }
}