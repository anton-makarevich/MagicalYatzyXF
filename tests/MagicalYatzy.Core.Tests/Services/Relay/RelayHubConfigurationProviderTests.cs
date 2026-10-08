using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NSubstitute;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.MagicalYatzy.Services.StorageService;
using Sanet.Transport.SignalR.Client.Relay;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.Services.Relay;

public class RelayHubConfigurationProviderTests
{
    private const string StorageKey = "HubConfigurations";
    private const string BuiltInHubId = "default";

    private readonly ISettingsStorageService _storage = Substitute.For<ISettingsStorageService>();
    private readonly IRelaySettings _settings = Substitute.For<IRelaySettings>();

    public RelayHubConfigurationProviderTests()
    {
        _settings.BaseUrl.Returns("https://relay.example.test");
        _settings.ApiKey.Returns("build-key");
    }

    [Fact]
    public async Task ConstructorSeedsBuiltInHubFromSettings()
    {
        var sut = CreateSut();

        var hubs = await sut.GetHubs();

        hubs.Count.ShouldBe(1);
        var hub = hubs[0];
        hub.Id.ShouldBe(BuiltInHubId);
        hub.Name.ShouldBe("Relay Hub");
        hub.IsBuiltIn.ShouldBeTrue();
        hub.BaseUrl.ShouldBe("https://relay.example.test");
        hub.ApiKey.ShouldBe("build-key");
        (await sut.GetActiveHubId()).ShouldBe(BuiltInHubId);
    }

    [Fact]
    public async Task GetActiveOptionsReturnsBuiltInHubValues()
    {
        var sut = CreateSut();

        var options = await sut.GetActiveOptions();

        options.BaseUrl.ShouldBe("https://relay.example.test");
        options.ApiKey.ShouldBe("build-key");
    }

    [Fact]
    public async Task GetActiveOptionsUsesCurrentSettingsValuesForBuiltInHub()
    {
        var sut = CreateSut();
        _settings.BaseUrl.Returns("https://updated.example");
        _settings.ApiKey.Returns("updated-key");

        var options = await sut.GetActiveOptions();

        options.BaseUrl.ShouldBe("https://updated.example");
        options.ApiKey.ShouldBe("updated-key");
    }

    [Fact]
    public async Task LoadsPersistedHubsAndSelection()
    {
        SeedStorage(StateJson(
            [UserHub("work", "Work Hub", "https://work.test", "work-key")],
            "work"));
        var sut = CreateSut();

        var hubs = await sut.GetHubs();

        hubs.Count.ShouldBe(2);
        hubs[0].Id.ShouldBe(BuiltInHubId);
        hubs[1].Id.ShouldBe("work");
        (await sut.GetActiveHubId()).ShouldBe("work");
        var options = await sut.GetActiveOptions();
        options.BaseUrl.ShouldBe("https://work.test");
        options.ApiKey.ShouldBe("work-key");
    }

    [Fact]
    public async Task IgnoresInvalidPersistedEntries()
    {
        SeedStorage(StateJson(
            [
                UserHub(" ", "Blank", "https://blank.test", ""),
                new HubConfigData(BuiltInHubId, "Impostor", "https://impostor.test", "", false),
                UserHub("dup", "First", "https://first.test", ""),
                UserHub("dup", "Second", "https://second.test", ""),
                new HubConfigData("usr", "Legacy", "https://legacy.test", "", true)
            ],
            "usr"));
        var sut = CreateSut();

        var hubs = await sut.GetHubs();

        hubs.Count.ShouldBe(3);
        hubs[0].Id.ShouldBe(BuiltInHubId);
        hubs[1].Id.ShouldBe("dup");
        hubs[1].Name.ShouldBe("First");
        hubs[2].Id.ShouldBe("usr");
        hubs[2].Name.ShouldBe("Legacy");
        hubs[2].IsBuiltIn.ShouldBeFalse();
        (await sut.GetActiveHubId()).ShouldBe("usr");
    }

    [Fact]
    public async Task FallsBackToBuiltInWhenPersistedSelectionIsUnknown()
    {
        SeedStorage(StateJson([UserHub("work", "Work", "https://work.test", "")], "ghost"));
        var sut = CreateSut();

        (await sut.GetActiveHubId()).ShouldBe(BuiltInHubId);
    }

    [Fact]
    public async Task KeepsBuiltInHubWhenPersistedStateIsMalformed()
    {
        SeedStorage("{ not json");
        var sut = CreateSut();

        var hubs = await sut.GetHubs();

        hubs.Count.ShouldBe(1);
        hubs[0].Id.ShouldBe(BuiltInHubId);
        (await sut.GetActiveHubId()).ShouldBe(BuiltInHubId);
    }

    [Fact]
    public async Task KeepsBuiltInHubWhenStorageReadFails()
    {
        _storage.LoadValueAsync(Arg.Any<string>())
            .Returns(Task.FromException<string?>(new IOException("read failed")));
        var sut = CreateSut();

        var hubs = await sut.GetHubs();

        hubs.Count.ShouldBe(1);
        hubs[0].Id.ShouldBe(BuiltInHubId);
    }

    [Fact]
    public async Task AddHubPersistsConfiguration()
    {
        var sut = CreateSut();

        await sut.AddHub(UserHub("work", "Work", "https://work.test", "key"));

        await _storage.Received(1).SaveValueAsync(StorageKey, Arg.Any<string>());
        var hubs = await sut.GetHubs();
        hubs.Count.ShouldBe(2);
        hubs.ShouldContain(h => h.Id == "work");
    }

    [Fact]
    public async Task AddHubRejectsBlankId()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<ArgumentException>(() =>
            sut.AddHub(UserHub(" ", "Work", "https://work.test", "")));

        exception.ParamName.ShouldBe("hub");
    }

    [Fact]
    public async Task AddHubRejectsDuplicateId()
    {
        var sut = CreateSut();
        await sut.AddHub(UserHub("work", "Work", "https://work.test", ""));

        await Should.ThrowAsync<ArgumentException>(() =>
            sut.AddHub(UserHub("work", "Work Again", "https://work2.test", "")));
    }

    [Fact]
    public async Task AddHubRejectsReservedBuiltInId()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<ArgumentException>(() =>
            sut.AddHub(UserHub(BuiltInHubId, "Fake", "https://fake.test", "")));

        (await sut.GetHubs()).Count.ShouldBe(1);
    }

    [Fact]
    public async Task AddHubDoesNotPersistWhenStorageFails()
    {
        FailNextSave();
        var sut = CreateSut();

        await Should.ThrowAsync<IOException>(() =>
            sut.AddHub(UserHub("work", "Work", "https://work.test", "")));

        (await sut.GetHubs()).Count.ShouldBe(1);
    }

    [Fact]
    public async Task UpdateHubPersistsChanges()
    {
        var sut = CreateSut();
        await sut.AddHub(UserHub("work", "Work", "https://work.test", "old"));

        await sut.UpdateHub("work", "Renamed", "https://new.test", "new");

        await _storage.Received(2).SaveValueAsync(StorageKey, Arg.Any<string>());
        var updated = (await sut.GetHubs()).Single(h => h.Id == "work");
        updated.Name.ShouldBe("Renamed");
        updated.BaseUrl.ShouldBe("https://new.test");
        updated.ApiKey.ShouldBe("new");
    }

    [Fact]
    public async Task UpdateHubRejectsBuiltInHub()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<InvalidOperationException>(() =>
            sut.UpdateHub(BuiltInHubId, "Renamed", "https://new.test", ""));
    }

    [Fact]
    public async Task UpdateHubRejectsUnknownHub()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<ArgumentException>(() =>
            sut.UpdateHub("ghost", "Ghost", "https://ghost.test", ""));
    }

    [Fact]
    public async Task UpdateHubRestoresPreviousValuesWhenStorageFails()
    {
        var sut = CreateSut();
        await sut.AddHub(UserHub("work", "Work", "https://work.test", "old"));
        FailNextSave();

        await Should.ThrowAsync<IOException>(() =>
            sut.UpdateHub("work", "Renamed", "https://new.test", "new"));

        var restored = (await sut.GetHubs()).Single(h => h.Id == "work");
        restored.Name.ShouldBe("Work");
        restored.BaseUrl.ShouldBe("https://work.test");
        restored.ApiKey.ShouldBe("old");
    }

    [Fact]
    public async Task RemoveHubPersistsRemoval()
    {
        var sut = CreateSut();
        await sut.AddHub(UserHub("work", "Work", "https://work.test", ""));

        await sut.RemoveHub("work");

        await _storage.Received(2).SaveValueAsync(StorageKey, Arg.Any<string>());
        (await sut.GetHubs()).Count.ShouldBe(1);
    }

    [Fact]
    public async Task RemoveHubRejectsBuiltInHub()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<InvalidOperationException>(() => sut.RemoveHub(BuiltInHubId));
    }

    [Fact]
    public async Task RemoveHubRejectsUnknownHub()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<ArgumentException>(() => sut.RemoveHub("ghost"));
    }

    [Fact]
    public async Task RemovingActiveHubFallsBackToBuiltIn()
    {
        var sut = CreateSut();
        await sut.AddHub(UserHub("work", "Work", "https://work.test", ""));
        await sut.SelectHub("work");

        await sut.RemoveHub("work");

        (await sut.GetActiveHubId()).ShouldBe(BuiltInHubId);
        var options = await sut.GetActiveOptions();
        options.BaseUrl.ShouldBe("https://relay.example.test");
    }

    [Fact]
    public async Task RemoveHubRestoresHubAndSelectionWhenStorageFails()
    {
        var sut = CreateSut();
        await sut.AddHub(UserHub("work", "Work", "https://work.test", ""));
        await sut.SelectHub("work");
        FailNextSave();

        await Should.ThrowAsync<IOException>(() => sut.RemoveHub("work"));

        (await sut.GetHubs()).ShouldContain(h => h.Id == "work");
        (await sut.GetActiveHubId()).ShouldBe("work");
    }

    [Fact]
    public async Task SelectHubPersistsSelection()
    {
        var sut = CreateSut();
        await sut.AddHub(UserHub("work", "Work", "https://work.test", "key"));

        await sut.SelectHub("work");

        await _storage.Received(2).SaveValueAsync(StorageKey, Arg.Any<string>());
        (await sut.GetActiveHubId()).ShouldBe("work");
        var options = await sut.GetActiveOptions();
        options.BaseUrl.ShouldBe("https://work.test");
        options.ApiKey.ShouldBe("key");
    }

    [Fact]
    public async Task SelectHubRejectsUnknownHub()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<ArgumentException>(() => sut.SelectHub("ghost"));

        (await sut.GetActiveHubId()).ShouldBe(BuiltInHubId);
    }

    [Fact]
    public async Task SelectHubRestoresPreviousSelectionWhenStorageFails()
    {
        var sut = CreateSut();
        await sut.AddHub(UserHub("work", "Work", "https://work.test", ""));
        await sut.SelectHub("work");
        FailNextSave();

        await Should.ThrowAsync<IOException>(() => sut.SelectHub(BuiltInHubId));

        (await sut.GetActiveHubId()).ShouldBe("work");
    }

    [Fact]
    public async Task GetHubsReturnsBuiltInFirstThenOrderedByName()
    {
        var sut = CreateSut();
        await sut.AddHub(UserHub("zeta", "zeta", "https://zeta.test", ""));
        await sut.AddHub(UserHub("beta", "Beta", "https://beta.test", ""));

        var hubs = await sut.GetHubs();

        hubs.Select(h => h.Name).ShouldBe(["Relay Hub", "Beta", "zeta"]);
    }

    private RelayHubConfigurationProvider CreateSut() => new(_settings, _storage);

    private void SeedStorage(string json) =>
        _storage.LoadValueAsync(StorageKey).Returns(Task.FromResult<string?>(json));

    private void FailNextSave() =>
        _storage.SaveValueAsync(StorageKey, Arg.Any<string>())
            .Returns(Task.FromException(new IOException("write failed")));

    private static HubConfigData UserHub(string id, string name, string baseUrl, string apiKey) =>
        new(id, name, baseUrl, apiKey, false);

    private static string StateJson(HubConfigData[] hubs, string activeHubId) =>
        JsonSerializer.Serialize(new { Hubs = hubs, ActiveHubId = activeHubId });
}
