using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.Transport.SignalR.Client.Relay;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.Services.Relay;

public class RelayPublisherProviderTests
{
    [Theory]
    [InlineData("https://relay.example.test", "https://relay.example.test/hubs/relay")]
    [InlineData("https://relay.example.test/", "https://relay.example.test/hubs/relay")]
    [InlineData("http://localhost:8080/", "http://localhost:8080/hubs/relay")]
    public void ComposeHubUrlNormalizesTrailingSlashAndAppendsHubPath(
        string baseUrl, string expectedUrl)
    {
        RelayPublisherProvider.ComposeHubUrl(baseUrl).ShouldBe(expectedUrl);
    }

    [Fact]
    public async Task CreateResolvesActiveHubOptionsBeforeBuildingOptions()
    {
        var configurationProvider = Substitute.For<IRelayHubConfigurationProvider>();
        configurationProvider.GetActiveOptions().Returns(new RelayClientOptions
        {
            BaseUrl = "https://relay.example.test",
            ApiKey = "key"
        });
        var sut = new RelayPublisherProvider(configurationProvider);

        // A non-6-character room code makes the relay publisher constructor throw synchronously,
        // so the create call fails fast without any network activity.
        await Should.ThrowAsync<ArgumentException>(() =>
            sut.Create("TOOLONG", "ticket", CancellationToken.None));
        await configurationProvider.Received(1).GetActiveOptions();
    }
}
