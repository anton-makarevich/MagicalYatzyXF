using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.DependencyInjection;
using Sanet.MagicalYatzy.Online;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.MagicalYatzy.Services.StorageService;
using Sanet.Transport.SignalR.Client.Relay;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.DependencyInjection;

public class CoreServicesTests
{
    [Fact]
    public async Task RegisterServicesRegistersRelayServicesAsSingletons()
    {
        var services = new ServiceCollection();
        services.RegisterServices();

        services.Single(descriptor => descriptor.ServiceType == typeof(IRelaySettings))
            .Lifetime.ShouldBe(ServiceLifetime.Singleton);
        services.Single(descriptor => descriptor.ServiceType == typeof(IRelayHubConfigurationProvider))
            .Lifetime.ShouldBe(ServiceLifetime.Singleton);
        services.Single(descriptor => descriptor.ServiceType == typeof(ISettingsStorageService))
            .Lifetime.ShouldBe(ServiceLifetime.Singleton);
        services.Single(descriptor => descriptor.ServiceType == typeof(ISettingsStorageService))
            .ImplementationType.ShouldBe(typeof(FileSettingsStorageService));
        services.Single(descriptor => descriptor.ServiceType == typeof(IRelayRoomClient))
            .Lifetime.ShouldBe(ServiceLifetime.Singleton);
        services.Single(descriptor => descriptor.ServiceType == typeof(IRelayPublisherProvider))
            .Lifetime.ShouldBe(ServiceLifetime.Singleton);
        services.Single(descriptor => descriptor.ServiceType == typeof(CommandRegistry))
            .Lifetime.ShouldBe(ServiceLifetime.Singleton);
        services.Single(descriptor => descriptor.ServiceType == typeof(IOnlineHostSession))
            .Lifetime.ShouldBe(ServiceLifetime.Transient);

        await using var serviceProvider = services.BuildServiceProvider();
        var relayClient = serviceProvider.GetRequiredService<IRelayRoomClient>();
        relayClient.ShouldBeOfType<RelayRoomClient>();
        serviceProvider.GetRequiredService<IRelayRoomClient>().ShouldBeSameAs(relayClient);

        var hostSession = serviceProvider.GetRequiredService<IOnlineHostSession>();
        hostSession.ShouldBeOfType<OnlineHostSession>();
        var secondHostSession = serviceProvider.GetRequiredService<IOnlineHostSession>();
        secondHostSession.ShouldBeOfType<OnlineHostSession>();
        secondHostSession.ShouldNotBeSameAs(hostSession);

        services.Single(descriptor => descriptor.ServiceType == typeof(IRelayHubConfigurationProvider))
            .ImplementationType.ShouldBe(typeof(RelayHubConfigurationProvider));

        var settings = serviceProvider.GetRequiredService<IRelaySettings>();
        settings.BaseUrl.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void RegisterServicesKeepsPreRegisteredSettingsStorage()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISettingsStorageService, InMemorySettingsStorage>();

        services.RegisterServices();

        services.Single(descriptor => descriptor.ServiceType == typeof(ISettingsStorageService))
            .ImplementationType.ShouldBe(typeof(InMemorySettingsStorage));
    }

    private sealed class InMemorySettingsStorage : ISettingsStorageService
    {
        public Task<string?> LoadValueAsync(string key) => Task.FromResult<string?>(null);

        public Task SaveValueAsync(string key, string value) => Task.CompletedTask;
    }
}