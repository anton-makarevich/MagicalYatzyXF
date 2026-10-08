using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.Localization;
using Sanet.MVVM.Core.ViewModels;
using Sanet.Transport.SignalR.Client.Relay;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

/// <summary>
/// Row/edit view model for a single relay hub entry in the Settings screen.
/// The built-in hub cannot be edited or removed.
/// </summary>
public class HubEntryViewModel : BindableBase
{
    private readonly Func<HubEntryViewModel, Task>? _onSaved;
    private readonly Func<HubEntryViewModel, CancellationToken, Task<HubStatus>>? _checkStatus;
    private readonly ILocalizationService? _localizationService;
    private HubConfigData _hub;
    private string _editableName;
    private string _editableBaseUrl;
    private string _editableApiKey;
    private int _refreshGeneration;

    public HubEntryViewModel(
        HubConfigData hub,
        Func<HubEntryViewModel, Task>? onSaved = null,
        Func<HubEntryViewModel, CancellationToken, Task<HubStatus>>? checkStatus = null,
        ILocalizationService? localizationService = null)
    {
        _hub = hub;
        _onSaved = onSaved;
        _checkStatus = checkStatus;
        _localizationService = localizationService;
        _editableName = hub.Name;
        _editableBaseUrl = hub.BaseUrl;
        _editableApiKey = hub.ApiKey;

        StartEditingCommand = new AsyncCommand(StartEditing);
        SaveCommand = new AsyncCommand(Save);
        CancelCommand = new AsyncCommand(Cancel);
        RefreshStatusCommand = new AsyncCommand(() => RefreshStatusAsync());
    }

    public HubConfigData Hub => _hub;

    /// <summary>
    /// The hub configuration produced by the current edits, before it is committed.
    /// </summary>
    public HubConfigData PendingHub => _hub with
    {
        Name = string.IsNullOrWhiteSpace(EditableName) ? Name : EditableName.Trim(),
        BaseUrl = EditableBaseUrl.Trim(),
        ApiKey = EditableApiKey
    };

    public string Id => _hub.Id;
    public string Name => _hub.Name;
    public string BaseUrl => _hub.BaseUrl;
    public string ApiKey => _hub.ApiKey;
    public bool IsBuiltIn => _hub.IsBuiltIn;

    public bool CanEdit => !IsBuiltIn;
    public bool CanRemove => !IsBuiltIn;

    public bool IsEditing
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Reachability state of this hub, surfaced by the status badge.
    /// </summary>
    public HubStatus Status
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// True while a health probe for this hub is in flight.
    /// </summary>
    public bool IsCheckingStatus
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string StatusText => Status switch
    {
        HubStatus.Online => Localize("HubStatusOnlineText"),
        HubStatus.Offline => Localize("HubStatusOfflineText"),
        HubStatus.Checking => Localize("HubStatusCheckingText"),
        _ => Localize("HubStatusUnknownText")
    };

    public string BuiltInText => IsBuiltIn ? Localize("BuiltInHubLabel") : string.Empty;

    public string NameLabel => Localize("HubNameLabel");
    public string UrlLabel => Localize("HubUrlLabel");
    public string ApiKeyLabel => Localize("HubApiKeyLabel");
    public string EditLabel => Localize("EditHubLabel");
    public string SaveLabel => Localize("SaveHubLabel");
    public string CancelLabel => Localize("CancelHubLabel");
    public string RemoveLabel => Localize("RemoveHubLabel");
    public string RefreshLabel => Localize("RefreshHubLabel");

    public string EditableName
    {
        get => _editableName;
        set => SetProperty(ref _editableName, value);
    }

    public string EditableBaseUrl
    {
        get => _editableBaseUrl;
        set => SetProperty(ref _editableBaseUrl, value);
    }

    public string EditableApiKey
    {
        get => _editableApiKey;
        set => SetProperty(ref _editableApiKey, value);
    }

    public ICommand StartEditingCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshStatusCommand { get; }

    public Task StartEditing()
    {
        if (!CanEdit) return Task.CompletedTask;

        EditableName = Name;
        EditableBaseUrl = BaseUrl;
        EditableApiKey = _hub.ApiKey;
        IsEditing = true;
        return Task.CompletedTask;
    }

    private async Task Save()
    {
        if (!HubUrlValidator.IsValid(EditableBaseUrl)) return;

        // Persist first; only commit the saved state and close the editor on success,
        // so a failed write keeps the edited values available for retry.
        if (_onSaved != null)
        {
            await _onSaved(this);
        }

        _hub = PendingHub;
        IsEditing = false;

        NotifyPropertyChanged(nameof(Hub));
        NotifyPropertyChanged(nameof(Name));
        NotifyPropertyChanged(nameof(BaseUrl));
    }

    private Task Cancel()
    {
        EditableName = Name;
        EditableBaseUrl = BaseUrl;
        EditableApiKey = _hub.ApiKey;
        IsEditing = false;
        return Task.CompletedTask;
    }

    public async Task RefreshStatusAsync(CancellationToken cancellationToken = default)
    {
        if (_checkStatus == null)
        {
            Status = HubStatus.Unknown;
            return;
        }

        var generation = Interlocked.Increment(ref _refreshGeneration);
        IsCheckingStatus = true;
        Status = HubStatus.Checking;
        try
        {
            var result = await _checkStatus(this, cancellationToken);
            if (generation != Volatile.Read(ref _refreshGeneration)) return;
            Status = result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (generation != Volatile.Read(ref _refreshGeneration)) return;
            Status = HubStatus.Unknown;
        }
        catch
        {
            if (generation != Volatile.Read(ref _refreshGeneration)) return;
            Status = HubStatus.Offline;
        }
        finally
        {
            if (generation == Volatile.Read(ref _refreshGeneration))
            {
                IsCheckingStatus = false;
            }
        }
    }

    private string Localize(string key) => _localizationService?.GetString(key) ?? key;
}
