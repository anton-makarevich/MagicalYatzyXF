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

        sut.BaseUrl.ShouldBe(RelayDefaults.BaseUrl);
        sut.ApiKey.ShouldBe(RelayDefaults.ApiKey);
    }

    [NoRelayOverridesFact]
    public void FallsBackToLocalRelayWithoutBuildTimeOverrides()
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

/// <summary>
/// Runs the decorated test only in builds that did not override the relay defaults with
/// -p:RelayBaseUrl / -p:RelayApiKey, since only then do the hardcoded fallbacks apply.
/// </summary>
public sealed class NoRelayOverridesFactAttribute : FactAttribute
{
    public NoRelayOverridesFactAttribute()
    {
        if (RelayDefaults.BaseUrl != "http://localhost:8080" || !string.IsNullOrEmpty(RelayDefaults.ApiKey))
        {
            Skip = "Build passed RelayBaseUrl/RelayApiKey overrides.";
        }
    }
}