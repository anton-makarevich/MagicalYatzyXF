using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Sanet.Localization;
using Sanet.Localization.Providers;
using Sanet.MagicalYatzy.Avalonia.Services.Stubs;
using Sanet.MagicalYatzy.Dto.ApiConfigs;
using Sanet.MagicalYatzy.Dto.Services;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Api;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Media;
using Sanet.MagicalYatzy.Services.Navigation;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.MagicalYatzy.Services.StorageService;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.Transport.SignalR.Client.Relay;

namespace Sanet.MagicalYatzy.Avalonia.DependencyInjection;

public static class CoreServices
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddTransient<IDicePanel, DicePanel>();
        services.AddSingleton<IPlayerService, PlayerService>();
        services.AddSingleton<IExternalNavigationService, ExternalNavigationStub>();
        services.AddSingleton<IGameSettingsService, GameSettingsService>();
        services.AddSingleton<IRelaySettings, RelaySettings>();
        services.AddSingleton<IRelayHubConfigurationProvider, RelaySettingsHubConfigurationProvider>();
        services.AddSingleton<IRelayRoomClient>(serviceProvider => new RelayRoomClient(
            new HttpClient(),
            serviceProvider.GetRequiredService<IRelayHubConfigurationProvider>(),
            NullLogger<RelayRoomClient>.Instance));
        services.AddSingleton<IApiClient, AzureApiClient>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IWebService, WebService>();
        services.AddSingleton<IApiConfig, AzureDevConfig>();
        services.AddSingleton<IRulesService, RulesService>();
        services.AddSingleton<IDiceGenerator, RandomDiceGenerator>();
        services.AddLocalization(
            new EmbeddedResourcesProvider(
                typeof(ViewModels.Base.DicePanelViewModel).Assembly,
                "Sanet.MagicalYatzy.Resources.Strings"));
        services.AddSingleton<IStorageService, LocalJsonStorageService>();
        services.AddSingleton<ISoundsProvider, SoundsProviderStub>();
    }

    public static void RegisterViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainMenuViewModel, MainMenuViewModel>();
        services.AddTransient<SettingsViewModel, SettingsViewModel>();
        services.AddTransient<LobbyViewModel, LobbyViewModel>();
        services.AddTransient<GameViewModel, GameViewModel>();
        services.AddTransient<GameResultsViewModel, GameResultsViewModel>();
    }
}