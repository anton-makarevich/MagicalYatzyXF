using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.DependencyInjection;
using Sanet.MagicalYatzy.Services.Relay;
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
        services.Single(descriptor => descriptor.ServiceType == typeof(IRelayRoomClient))
            .Lifetime.ShouldBe(ServiceLifetime.Singleton);

        await using var serviceProvider = services.BuildServiceProvider();
        var relayClient = serviceProvider.GetRequiredService<IRelayRoomClient>();
        relayClient.ShouldBeOfType<RelayRoomClient>();
        serviceProvider.GetRequiredService<IRelayRoomClient>().ShouldBeSameAs(relayClient);

        var settings = serviceProvider.GetRequiredService<IRelaySettings>();
        settings.BaseUrl = "https://updated.example.test";
        var configurationProvider = serviceProvider.GetRequiredService<IRelayHubConfigurationProvider>();
        var options = await configurationProvider.GetActiveOptions();

        options.BaseUrl.ShouldBe("https://updated.example.test");
    }
}