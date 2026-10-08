using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using NSubstitute;
using Sanet.Localization;
using Sanet.MagicalYatzy.ViewModels;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.ViewModels;

public class AddHubViewModelTests
{
    private readonly AddHubViewModel _sut;
    private readonly ILocalizationService _localizationService;

    public AddHubViewModelTests()
    {
        _localizationService = Substitute.For<ILocalizationService>();
        _sut = new AddHubViewModel(_localizationService);
    }

    [Fact]
    public async Task Confirm_ShouldSetUrlRequiredValidation_WhenBaseUrlBlank()
    {
        _localizationService.GetString("HubUrlRequiredMessage").Returns("URL required");

        var resultTask = _sut.GetResultAsync();
        await ((IAsyncCommand)_sut.ConfirmCommand).ExecuteAsync();

        _sut.ValidationMessage.ShouldBe("URL required");
        _sut.HasValidationMessage.ShouldBeTrue();
        resultTask.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Confirm_ShouldSetUrlInvalidValidation_WhenBaseUrlNotAbsoluteHttp()
    {
        _localizationService.GetString("HubUrlInvalidMessage").Returns("Invalid URL");
        _sut.BaseUrl = "ftp://example.com";

        var resultTask = _sut.GetResultAsync();
        await ((IAsyncCommand)_sut.ConfirmCommand).ExecuteAsync();

        _sut.ValidationMessage.ShouldBe("Invalid URL");
        _sut.HasValidationMessage.ShouldBeTrue();
        resultTask.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Confirm_ShouldReturnTrimmedResult_WhenValid()
    {
        _sut.Name = "  My Hub  ";
        _sut.BaseUrl = "  http://hub.local  ";
        _sut.ApiKey = "  secret  ";

        var resultTask = _sut.GetResultAsync();
        await ((IAsyncCommand)_sut.ConfirmCommand).ExecuteAsync();

        var result = await resultTask;
        result.ShouldNotBeNull();
        result.Name.ShouldBe("My Hub");
        result.BaseUrl.ShouldBe("http://hub.local");
        result.ApiKey.ShouldBe("  secret  ");
        _sut.HasValidationMessage.ShouldBeFalse();
    }

    [Fact]
    public async Task BaseUrlChange_ShouldClearValidationMessage()
    {
        _localizationService.GetString("HubUrlInvalidMessage").Returns("Invalid URL");
        _sut.BaseUrl = "not a url";
        await ((IAsyncCommand)_sut.ConfirmCommand).ExecuteAsync();
        _sut.HasValidationMessage.ShouldBeTrue();

        _sut.BaseUrl = "http://hub.local";

        _sut.ValidationMessage.ShouldBeEmpty();
        _sut.HasValidationMessage.ShouldBeFalse();
    }

    [Fact]
    public async Task Cancel_ShouldCompleteWithNullResult()
    {
        var resultTask = _sut.GetResultAsync();

        await ((IAsyncCommand)_sut.CancelCommand).ExecuteAsync();

        var result = await resultTask;
        result.ShouldBeNull();
    }

    [Fact]
    public void Captions_ShouldReturnLocalizedStrings()
    {
        _localizationService.GetString("AddHubCaptionText").Returns("Add Hub");
        _localizationService.GetString("HubNameLabel").Returns("Name");
        _localizationService.GetString("HubUrlLabel").Returns("URL");
        _localizationService.GetString("HubApiKeyLabel").Returns("API key");
        _localizationService.GetString("ConfirmHubLabel").Returns("Confirm");
        _localizationService.GetString("CancelHubLabel").Returns("Cancel");

        _sut.Title.ShouldBe("Add Hub");
        _sut.NameLabel.ShouldBe("Name");
        _sut.UrlLabel.ShouldBe("URL");
        _sut.ApiKeyLabel.ShouldBe("API key");
        _sut.ConfirmLabel.ShouldBe("Confirm");
        _sut.CancelLabel.ShouldBe("Cancel");
    }
}
