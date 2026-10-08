using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using NSubstitute;
using Sanet.Localization;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Sanet.Transport.SignalR.Client.Relay;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.ViewModels;

public class HubEntryViewModelTests
{
    private static HubConfigData BuiltInHub => new("default", "Relay Hub", "http://builtin.local", string.Empty, true);

    private static HubConfigData UserHub => new("custom-1", "My Hub", "http://my-hub.local", "secret", false);

    [Fact]
    public void BuiltInHub_ShouldNotAllowEditingAndRemoving()
    {
        var sut = new HubEntryViewModel(BuiltInHub);

        sut.CanEdit.ShouldBeFalse();
        sut.CanRemove.ShouldBeFalse();
        sut.IsBuiltIn.ShouldBeTrue();
    }

    [Fact]
    public async Task StartEditing_ShouldRefuseBuiltInHub()
    {
        var sut = new HubEntryViewModel(BuiltInHub);

        await ((IAsyncCommand)sut.StartEditingCommand).ExecuteAsync();

        sut.IsEditing.ShouldBeFalse();
        sut.EditableName.ShouldBe("Relay Hub");
    }

    [Fact]
    public void UserHub_ShouldAllowEditingAndRemoving()
    {
        var sut = new HubEntryViewModel(UserHub);

        sut.CanEdit.ShouldBeTrue();
        sut.CanRemove.ShouldBeTrue();
    }

    [Fact]
    public async Task StartEditing_ShouldCopyCommittedValuesIntoEditableFields()
    {
        var sut = new HubEntryViewModel(UserHub);

        await ((IAsyncCommand)sut.StartEditingCommand).ExecuteAsync();

        sut.IsEditing.ShouldBeTrue();
        sut.EditableName.ShouldBe("My Hub");
        sut.EditableBaseUrl.ShouldBe("http://my-hub.local");
        sut.EditableApiKey.ShouldBe("secret");
    }

    [Fact]
    public async Task Save_ShouldCallPersistenceCallbackAndCommit()
    {
        var saveCalled = false;
        HubEntryViewModel? savedEntry = null;
        var sut = new HubEntryViewModel(UserHub, onSaved: entry =>
        {
            saveCalled = true;
            savedEntry = entry;
            return Task.CompletedTask;
        });

        await ((IAsyncCommand)sut.StartEditingCommand).ExecuteAsync();
        sut.EditableName = "  Renamed  ";
        sut.EditableBaseUrl = "  http://new.local  ";
        sut.EditableApiKey = "new-key";

        await ((IAsyncCommand)sut.SaveCommand).ExecuteAsync();

        saveCalled.ShouldBeTrue();
        savedEntry.ShouldBeSameAs(sut);
        sut.IsEditing.ShouldBeFalse();
        sut.Name.ShouldBe("Renamed");
        sut.BaseUrl.ShouldBe("http://new.local");
        sut.ApiKey.ShouldBe("new-key");
    }

    [Fact]
    public async Task Save_ShouldUseTrimmedUrl_KeepExistingName_WhenNameBlank()
    {
        var sut = new HubEntryViewModel(UserHub);

        await ((IAsyncCommand)sut.StartEditingCommand).ExecuteAsync();
        sut.EditableName = "   ";
        sut.EditableBaseUrl = "  http://new.local  ";
        sut.EditableApiKey = "new-key";

        sut.PendingHub.Name.ShouldBe("My Hub");
        sut.PendingHub.BaseUrl.ShouldBe("http://new.local");
        sut.PendingHub.ApiKey.ShouldBe("new-key");
    }

    [Fact]
    public async Task Save_ShouldNotCallCallback_WhenUrlIsBlank()
    {
        var saveCalled = false;
        var sut = new HubEntryViewModel(UserHub, onSaved: _ =>
        {
            saveCalled = true;
            return Task.CompletedTask;
        });

        await ((IAsyncCommand)sut.StartEditingCommand).ExecuteAsync();
        sut.EditableBaseUrl = "   ";

        await ((IAsyncCommand)sut.SaveCommand).ExecuteAsync();

        saveCalled.ShouldBeFalse();
        sut.IsEditing.ShouldBeTrue();
    }

    [Fact]
    public async Task Save_ShouldNotCallCallback_WhenUrlIsNotAbsoluteHttp()
    {
        var saveCalled = false;
        var sut = new HubEntryViewModel(UserHub, onSaved: _ =>
        {
            saveCalled = true;
            return Task.CompletedTask;
        });

        await ((IAsyncCommand)sut.StartEditingCommand).ExecuteAsync();
        sut.EditableBaseUrl = "ftp://not-a-valid-url";

        await ((IAsyncCommand)sut.SaveCommand).ExecuteAsync();

        saveCalled.ShouldBeFalse();
        sut.IsEditing.ShouldBeTrue();
    }

    [Fact]
    public async Task Save_WhenPersistenceFails_ShouldKeepEditorOpenWithEdits()
    {
        var sut = new HubEntryViewModel(UserHub, onSaved: _ => Task.FromException(new Exception("persist failed")));

        await ((IAsyncCommand)sut.StartEditingCommand).ExecuteAsync();
        sut.EditableName = "Renamed";
        sut.EditableBaseUrl = "http://new.local";
        sut.EditableApiKey = "new-key";

        await Should.ThrowAsync<Exception>(() => ((IAsyncCommand)sut.SaveCommand).ExecuteAsync());

        sut.IsEditing.ShouldBeTrue();
        sut.EditableName.ShouldBe("Renamed");
        sut.EditableBaseUrl.ShouldBe("http://new.local");
        sut.EditableApiKey.ShouldBe("new-key");
        sut.Name.ShouldBe("My Hub");
        sut.BaseUrl.ShouldBe("http://my-hub.local");
    }

    [Fact]
    public async Task Cancel_ShouldRestoreOriginalValuesAndCloseEditor()
    {
        var sut = new HubEntryViewModel(UserHub);

        await ((IAsyncCommand)sut.StartEditingCommand).ExecuteAsync();
        sut.EditableName = "Renamed";
        sut.EditableBaseUrl = "http://new.local";

        await ((IAsyncCommand)sut.CancelCommand).ExecuteAsync();

        sut.IsEditing.ShouldBeFalse();
        sut.EditableName.ShouldBe("My Hub");
        sut.EditableBaseUrl.ShouldBe("http://my-hub.local");
        sut.EditableApiKey.ShouldBe("secret");
    }

    [Fact]
    public async Task RefreshStatus_ShouldMapNullHealthResultToOnline()
    {
        Func<HubEntryViewModel, CancellationToken, Task<HubStatus>> checkStatus =
            (_, _) => Task.FromResult(HubStatus.Online);
        var sut = new HubEntryViewModel(UserHub, checkStatus: checkStatus);

        await sut.RefreshStatusAsync();

        sut.Status.ShouldBe(HubStatus.Online);
        sut.IsCheckingStatus.ShouldBeFalse();
    }

    [Fact]
    public async Task RefreshStatus_ShouldMapDelegateFailureToOffline()
    {
        Func<HubEntryViewModel, CancellationToken, Task<HubStatus>> checkStatus =
            (_, _) => throw new Exception("probe failed");
        var sut = new HubEntryViewModel(UserHub, checkStatus: checkStatus);

        await sut.RefreshStatusAsync();

        sut.Status.ShouldBe(HubStatus.Offline);
    }

    [Fact]
    public async Task RefreshStatus_ShouldMapCancellationToUnknown()
    {
        var cts = new CancellationTokenSource();
        Func<HubEntryViewModel, CancellationToken, Task<HubStatus>> checkStatus = (_, token) =>
        {
            cts.Cancel();
            token.ThrowIfCancellationRequested();
            return Task.FromResult(HubStatus.Online);
        };
        var sut = new HubEntryViewModel(UserHub, checkStatus: checkStatus);

        await sut.RefreshStatusAsync(cts.Token);

        sut.Status.ShouldBe(HubStatus.Unknown);
        sut.IsCheckingStatus.ShouldBeFalse();
    }

    [Fact]
    public async Task RefreshStatus_ShouldIgnoreStaleProbeResults_NewerProbeWins()
    {
        var firstProbe = new TaskCompletionSource<HubStatus>();
        var firstActive = false;
        Func<HubEntryViewModel, CancellationToken, Task<HubStatus>> checkStatus = (_, _) =>
        {
            if (!firstActive)
            {
                firstActive = true;
                return firstProbe.Task;
            }

            return Task.FromResult(HubStatus.Online);
        };
        var sut = new HubEntryViewModel(UserHub, checkStatus: checkStatus);

        var staleProbe = sut.RefreshStatusAsync();
        await sut.RefreshStatusAsync();
        firstProbe.TrySetResult(HubStatus.Offline);
        await staleProbe;

        sut.Status.ShouldBe(HubStatus.Online);
        sut.IsCheckingStatus.ShouldBeFalse();
    }

    [Fact]
    public async Task RefreshStatus_ShouldSetCheckingWhileProbeIsInFlight()
    {
        var probeGate = new TaskCompletionSource<HubStatus>();
        var sut = new HubEntryViewModel(UserHub, checkStatus: (_, _) => probeGate.Task);

        var probe = sut.RefreshStatusAsync();

        sut.Status.ShouldBe(HubStatus.Checking);
        sut.IsCheckingStatus.ShouldBeTrue();

        probeGate.TrySetResult(HubStatus.Offline);
        await probe;

        sut.Status.ShouldBe(HubStatus.Offline);
        sut.IsCheckingStatus.ShouldBeFalse();
    }

    [Fact]
    public void StatusText_ShouldReturnLocalizedString()
    {
        var localizationService = Substitute.For<ILocalizationService>();
        localizationService.GetString("HubStatusOnlineText").Returns("Online");
        var sut = new HubEntryViewModel(UserHub, localizationService: localizationService);

        sut.Status = HubStatus.Online;

        sut.StatusText.ShouldBe("Online");
    }

    [Fact]
    public void BuiltInText_ShouldReturnLocalizedString_ForBuiltInHubOnly()
    {
        var localizationService = Substitute.For<ILocalizationService>();
        localizationService.GetString("BuiltInHubLabel").Returns("Built-in");
        var builtIn = new HubEntryViewModel(BuiltInHub, localizationService: localizationService);
        var user = new HubEntryViewModel(UserHub, localizationService: localizationService);

        builtIn.BuiltInText.ShouldBe("Built-in");
        user.BuiltInText.ShouldBeEmpty();
    }
}
