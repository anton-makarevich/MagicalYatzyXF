using System;
using System.IO;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Services.StorageService;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.Services;

public class FileSettingsStorageServiceTests : IDisposable
{
    private readonly string _baseFolder =
        Path.Combine(Path.GetTempPath(), "MagicalYatzyTests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_baseFolder))
            Directory.Delete(_baseFolder, true);
    }

    [Fact]
    public async Task LoadValueReturnsNullForMissingKey()
    {
        var sut = CreateSut();

        var result = await sut.LoadValueAsync("missing");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task SaveThenLoadReturnsValue()
    {
        var sut = CreateSut();

        await sut.SaveValueAsync("HubConfigurations", "{\"some\":\"json\"}");
        var result = await sut.LoadValueAsync("HubConfigurations");

        result.ShouldBe("{\"some\":\"json\"}");
    }

    [Fact]
    public async Task SaveCreatesMissingFolder()
    {
        var sut = CreateSut();

        await sut.SaveValueAsync("key", "value");

        Directory.Exists(_baseFolder).ShouldBeTrue();
    }

    [Fact]
    public async Task ValuesAreStoredPerKey()
    {
        var sut = CreateSut();
        await sut.SaveValueAsync("first", "one");
        await sut.SaveValueAsync("second", "two");

        (await sut.LoadValueAsync("first")).ShouldBe("one");
        (await sut.LoadValueAsync("second")).ShouldBe("two");
    }

    [Fact]
    public async Task SaveOverwritesPreviousValue()
    {
        var sut = CreateSut();
        await sut.SaveValueAsync("key", "old");

        await sut.SaveValueAsync("key", "new");

        (await sut.LoadValueAsync("key")).ShouldBe("new");
    }

    [Fact]
    public async Task SaveSupportsKeysWithInvalidFileNameCharacters()
    {
        var sut = CreateSut();

        await sut.SaveValueAsync("hub:config/with*chars?", "value");

        (await sut.LoadValueAsync("hub:config/with*chars?")).ShouldBe("value");
    }

    [Fact]
    public async Task SavePropagatesWriteErrors()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"MagicalYatzyFile_{Guid.NewGuid():N}");
        File.WriteAllText(filePath, "occupied");
        try
        {
            var sut = new FileSettingsStorageService(filePath);

            await Should.ThrowAsync<IOException>(() => sut.SaveValueAsync("key", "value"));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private FileSettingsStorageService CreateSut() => new(_baseFolder);
}
