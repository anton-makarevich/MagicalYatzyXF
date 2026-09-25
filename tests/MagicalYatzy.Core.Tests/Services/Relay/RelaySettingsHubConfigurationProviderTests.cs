using System.Threading.Tasks;
using Sanet.MagicalYatzy.Services.Relay;
using Shouldly;
using Xunit;

namespace MagicalYatzyTests.Services.Relay;

public class RelaySettingsHubConfigurationProviderTests
{
    [Fact]
    public async Task GetsCurrentSettingsWhenResolvingActiveOptions()
    {
        var settings = new RelaySettings
        {
            BaseUrl = "https://relay.example.test",
            ApiKey = "initial-key"
        };
        var sut = new RelaySettingsHubConfigurationProvider(settings);

        var initialOptions = await sut.GetActiveOptions();
        settings.BaseUrl = "https://updated.example.test";
        settings.ApiKey = "updated-key";
        var updatedOptions = await sut.GetActiveOptions();

        initialOptions.BaseUrl.ShouldBe("https://relay.example.test");
        initialOptions.ApiKey.ShouldBe("initial-key");
        updatedOptions.BaseUrl.ShouldBe("https://updated.example.test");
        updatedOptions.ApiKey.ShouldBe("updated-key");
    }

    [Fact]
    public async Task UpdatingTheSingleHubWritesBackToSettings()
    {
        var settings = new RelaySettings();
        var sut = new RelaySettingsHubConfigurationProvider(settings);

        await sut.UpdateHub("default", "Relay Hub", "https://relay.example.test", "updated-key");

        settings.BaseUrl.ShouldBe("https://relay.example.test");
        settings.ApiKey.ShouldBe("updated-key");
        (await sut.GetHubs()).ShouldHaveSingleItem();
    }
}