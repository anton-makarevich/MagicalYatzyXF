using Sanet.MagicalYatzy.Services.Relay;
using Shouldly;
using Xunit;

namespace MagicalYatzyTests.Services.Relay;

public class RelaySettingsTests
{
    [Fact]
    public void InitializesFromRelayDefaults()
    {
        var sut = new RelaySettings();

        sut.BaseUrl.ShouldBe("http://localhost:8080");
        sut.ApiKey.ShouldBeEmpty();
    }

    [Fact]
    public void StoresEditedValues()
    {
        var sut = new RelaySettings
        {
            BaseUrl = "https://relay.example.test",
            ApiKey = "test-key"
        };

        sut.BaseUrl.ShouldBe("https://relay.example.test");
        sut.ApiKey.ShouldBe("test-key");
    }
}