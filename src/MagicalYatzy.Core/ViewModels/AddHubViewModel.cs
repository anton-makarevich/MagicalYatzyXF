using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.Localization;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MagicalYatzy.ViewModels;

/// <summary>
/// Modal dialog view model for adding a new relay hub.
/// Collects the hub name, base URL and API key and returns them
/// as an <see cref="AddHubResult"/> to the caller.
/// </summary>
public class AddHubViewModel : BaseViewModel, IResultProvider<AddHubResult?>
{
    private readonly TaskCompletionSource<AddHubResult?> _resultTaskCompletionSource = new();
    private readonly ILocalizationService _localizationService;

    public AddHubViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        ConfirmCommand = new AsyncCommand(Confirm);
        CancelCommand = new AsyncCommand(Cancel);
    }

    public string Title => _localizationService.GetString("AddHubCaptionText");

    public string NameLabel => _localizationService.GetString("HubNameLabel");

    public string UrlLabel => _localizationService.GetString("HubUrlLabel");

    public string ApiKeyLabel => _localizationService.GetString("HubApiKeyLabel");

    public string ConfirmLabel => _localizationService.GetString("ConfirmHubLabel");

    public string CancelLabel => _localizationService.GetString("CancelHubLabel");

    public string Name
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string BaseUrl
    {
        get;
        set
        {
            SetProperty(ref field, value);
            ValidationMessage = string.Empty;
        }
    } = string.Empty;

    public string ApiKey
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string ValidationMessage
    {
        get;
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(HasValidationMessage));
        }
    } = string.Empty;

    public bool HasValidationMessage => !string.IsNullOrEmpty(ValidationMessage);

    public ICommand ConfirmCommand { get; }

    public ICommand CancelCommand { get; }

    public Task<AddHubResult?> GetResultAsync() => _resultTaskCompletionSource.Task;

    private Task Confirm()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            ValidationMessage = _localizationService.GetString("HubUrlRequiredMessage");
            return Task.CompletedTask;
        }

        if (!HubUrlValidator.IsValid(BaseUrl))
        {
            ValidationMessage = _localizationService.GetString("HubUrlInvalidMessage");
            return Task.CompletedTask;
        }

        _resultTaskCompletionSource.TrySetResult(new AddHubResult
        {
            Name = string.IsNullOrWhiteSpace(Name) ? string.Empty : Name.Trim(),
            BaseUrl = BaseUrl.Trim(),
            ApiKey = ApiKey
        });
        return Task.CompletedTask;
    }

    private Task Cancel()
    {
        _resultTaskCompletionSource.TrySetResult(null);
        return Task.CompletedTask;
    }
}
