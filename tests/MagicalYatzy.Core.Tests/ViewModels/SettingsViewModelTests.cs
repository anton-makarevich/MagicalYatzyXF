using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using NSubstitute;
using Sanet.Localization;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Sanet.MVVM.Core.Services;
using Sanet.Transport.SignalR.Client.Relay;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.ViewModels;

public class SettingsViewModelTests
{
    private readonly SettingsViewModel _sut;
    private readonly IGameSettingsService _gameSettingsService;
    private readonly ILocalizationService _localizationService;
    private readonly IRelayHubConfigurationProvider _hubConfigurationProvider = Substitute.For<IRelayHubConfigurationProvider>();
    private readonly IRelayRoomClient _relayRoomClient = Substitute.For<IRelayRoomClient>();

    private readonly Language _defLanguage = new Language("en", true, "english");

    public SettingsViewModelTests()
    {
        var dicePanel = Substitute.For<IDicePanel>();
        _gameSettingsService = Substitute.For<IGameSettingsService>();
        _localizationService = Substitute.For<ILocalizationService>();

        _localizationService.Languages.Returns(new List<Language> {_defLanguage});

        _sut = new SettingsViewModel(
            dicePanel,
            _gameSettingsService,
            _localizationService,
            _hubConfigurationProvider,
            _relayRoomClient);
    }

    private static HubConfigData DemoHub => new("default", "Relay Hub", "http://demo.local", string.Empty, true);

    private static HubConfigData CustomHub => new("custom-1", "My Hub", "http://my-hub.example", "secret", false);

    private static bool HasNewGuidId(HubConfigData hub) => Guid.TryParseExact(hub.Id, "N", out _);

    private void SetupProviderHubs(IReadOnlyList<HubConfigData> hubs, string activeHubId)
    {
        _hubConfigurationProvider.GetHubs().Returns(Task.FromResult(hubs));
        _hubConfigurationProvider.GetActiveHubId().Returns(Task.FromResult(activeHubId));
    }

    private static async Task WaitFor(Func<bool> condition, int timeoutMs = 2000, int intervalMs = 10)
    {
        var start = DateTime.UtcNow;
        while (!condition())
        {
            if ((DateTime.UtcNow - start).TotalMilliseconds > timeoutMs)
                throw new TimeoutException("Condition not met within timeout");
            await Task.Delay(intervalMs);
        }
    }

    [Fact]
    public void Title_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedTitle = "SettingsCaptionText";

        _localizationService.GetString("SettingsCaptionText").Returns(expectedTitle);

        // Act
        var title = _sut.Title;

        // Assert
        title.ShouldBe(expectedTitle);
    }
    
    [Fact]
    public void LanguageLabel_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expected = "Language";

        _localizationService.GetString("LanguageLabel").Returns(expected);

        // Act
        var language = _sut.LanguageLabel;

        // Assert
        language.ShouldBe(expected);
    }

    [Fact]
    public void IsStyleBlue_ShouldSetDieStyleToBlue_WhenValueIsTrue()
    {
        // Act
        _sut.IsStyleBlue = true;

        // Assert
        _gameSettingsService.DieStyle.ShouldBe(DiceStyle.Blue);
    }

    [Fact]
    public void IsStyleRed_ShouldSetDieStyleToRed_WhenValueIsTrue()
    {
        // Act
        _sut.IsStyleRed = true;

        // Assert
        _gameSettingsService.DieStyle.ShouldBe(DiceStyle.Red);
    }

    [Fact]
    public void IsStyleWhite_ShouldSetDieStyleToClassic_WhenValueIsTrue()
    {
        // Act
        _sut.IsStyleWhite = true;

        // Assert
        _gameSettingsService.DieStyle.ShouldBe(DiceStyle.Classic);
    }

        [Fact]
    public void IsSpeedVerySlow_ShouldSetDieSpeedToVerySlow_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedVerySlow = true;

        // Assert
        _gameSettingsService.DieSpeed.ShouldBe((int)DiceSpeed.VerySlow);
    }

    [Fact]
    public void IsSpeedSlow_ShouldSetDieSpeedToSlow_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedSlow = true;

        // Assert
        _gameSettingsService.DieSpeed.ShouldBe((int)DiceSpeed.Slow);
    }

    [Fact]
    public void IsSpeedFast_ShouldSetDieSpeedToFast_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedFast = true;

        // Assert
        _gameSettingsService.DieSpeed.ShouldBe((int)DiceSpeed.Fast);
    }

    [Fact]
    public void IsSpeedVeryFast_ShouldSetDieSpeedToVeryFast_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedVeryFast = true;

        // Assert
        _gameSettingsService.DieSpeed.ShouldBe((int)DiceSpeed.VeryFast);
    }

    [Fact]
    public void IsAngLow_ShouldSetDieAngleTo0_WhenValueIsTrue()
    {
        // Act
        _sut.IsAngleLow = true;

        // Assert
        _gameSettingsService.DieAngle.ShouldBe(0);
    }

    [Fact]
    public void IsAngHigh_ShouldSetDieAngleTo2_WhenValueIsTrue()
    {
        // Act
        _sut.IsAngleHigh = true;

        // Assert
        _gameSettingsService.DieAngle.ShouldBe(2);
    }

    [Fact]
    public void IsAngVeryHigh_ShouldSetDieAngleTo4_WhenValueIsTrue()
    {
        // Act
        _sut.IsAngleVeryHigh = true;

        // Assert
        _gameSettingsService.DieAngle.ShouldBe(4);
    }

    [Fact]
    public void IsSoundEnabled_ShouldReturnIsSoundEnabledFromGameSettingsService()
    {
        // Arrange
        const bool expectedIsSoundEnabled = true;

        _gameSettingsService.IsSoundEnabled.Returns(expectedIsSoundEnabled);

        // Act
        var isSoundEnabled = _sut.IsSoundEnabled;

        // Assert
        isSoundEnabled.ShouldBe(expectedIsSoundEnabled);
    }

    [Fact]
    public void IsSoundEnabled_ShouldSetIsSoundEnabledInGameSettingsService_WhenValueChanged()
    {
        // Arrange
        const bool expectedIsSoundEnabled = true;

        // Act
        _sut.IsSoundEnabled = expectedIsSoundEnabled;

        // Assert
        _gameSettingsService.IsSoundEnabled.ShouldBe(expectedIsSoundEnabled);
    }

    [Fact]
    public void SoundLabel_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSoundLabel = "SoundLabel";

        _localizationService.GetString("SoundLabel").Returns(expectedSoundLabel);

        // Act
        var soundLabel = _sut.SoundLabel;

        // Assert
        soundLabel.ShouldBe(expectedSoundLabel);
    }

    [Fact]
    public void OffContent_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedOffContent = "OffContent";

        _localizationService.GetString("OffContent").Returns(expectedOffContent);

        // Act
        var offContent = _sut.OffContent;

        // Assert
        offContent.ShouldBe(expectedOffContent);
    }

    [Fact]
    public void OnContent_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedOnContent = "OnContent";

        _localizationService.GetString("OnContent").Returns(expectedOnContent);

        // Act
        var onContent = _sut.OnContent;

        // Assert
        onContent.ShouldBe(expectedOnContent);
    }
    
    [Fact]
    public void SettingsStyleCaption_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSettingsStyleCaption = "SettingsStyleCaptionText";

        _localizationService.GetString("SettingsStyleCaptionText").Returns(expectedSettingsStyleCaption);

        // Act
        var settingsStyleCaption = _sut.SettingsStyleCaption;

        // Assert
        settingsStyleCaption.ShouldBe(expectedSettingsStyleCaption);
    }

    [Fact]
    public void AngLowText_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedAngLowText = "AngLowText";

        _localizationService.GetString("AngLowText").Returns(expectedAngLowText);

        // Act
        var angLowText = _sut.AngleLowText;

        // Assert
        angLowText.ShouldBe(expectedAngLowText);
    }

    [Fact]
    public void AngHighText_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedAngHighText = "AngHighText";

        _localizationService.GetString("AngHighText").Returns(expectedAngHighText);

        // Act
        var angHighText = _sut.AngleHighText;

        // Assert
        angHighText.ShouldBe(expectedAngHighText);
    }

    [Fact]
    public void AngVeryHighText_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedAngVeryHighText = "AngVeryHighText";

        _localizationService.GetString("AngVeryHighText").Returns(expectedAngVeryHighText);

        // Act
        var angVeryHighText = _sut.AngleVeryHighText;

        // Assert
        angVeryHighText.ShouldBe(expectedAngVeryHighText);
    }

    [Fact]
    public void SettingsAngleCaption_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSettingsAngleCaption = "SettingsAngleCaptionText";

        _localizationService.GetString("SettingsAngleCaptionText").Returns(expectedSettingsAngleCaption);

        // Act
        var settingsAngleCaption = _sut.SettingsAngleCaption;

        // Assert
        settingsAngleCaption.ShouldBe(expectedSettingsAngleCaption);
    }

    [Fact]
    public void SpeedSlow_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSpeedSlow = "SpeedSlowText";

        _localizationService.GetString("SpeedSlowText").Returns(expectedSpeedSlow);

        // Act
        var speedSlow = _sut.SpeedSlow;

        // Assert
        speedSlow.ShouldBe(expectedSpeedSlow);
    }

    [Fact]
    public void SpeedVerySlow_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSpeedVerySlow = "SpeedVerySlowText";

        _localizationService.GetString("SpeedVerySlowText").Returns(expectedSpeedVerySlow);

        // Act
        var speedVerySlow = _sut.SpeedVerySlow;

        // Assert
        speedVerySlow.ShouldBe(expectedSpeedVerySlow);
    }

    [Fact]
    public void SpeedFast_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSpeedFast = "SpeedFastText";

        _localizationService.GetString("SpeedFastText").Returns(expectedSpeedFast);

        // Act
        var speedFast = _sut.SpeedFast;

        // Assert
        speedFast.ShouldBe(expectedSpeedFast);
    }

    [Fact]
    public void SpeedVeryFast_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSpeedVeryFast = "SpeedVeryFastText";

        _localizationService.GetString("SpeedVeryFastText").Returns(expectedSpeedVeryFast);

        // Act
        var speedVeryFast = _sut.SpeedVeryFast;

        // Assert
        speedVeryFast.ShouldBe(expectedSpeedVeryFast);
    }

    [Fact]
    public void DieAngle_ShouldReturnDieAngleFromGameSettingsService()
    {
        // Arrange
        const int expectedDieAngle = 2;

        _gameSettingsService.DieAngle.Returns(expectedDieAngle);

        // Act
        var dieAngle = _sut.DieAngle;

        // Assert
        dieAngle.ShouldBe(expectedDieAngle);
    }

    [Fact]
    public void DieSpeed_ShouldReturnDieSpeedFromGameSettingsService()
    {
        // Arrange
        const int expectedDieSpeed = (int)DiceSpeed.VerySlow;

        _gameSettingsService.DieSpeed.Returns(expectedDieSpeed);

        // Act
        var dieSpeed = _sut.DieSpeed;

        // Assert
        dieSpeed.ShouldBe((DiceSpeed)expectedDieSpeed);
    }

    [Fact]
    public void DieStyle_ShouldReturnDieStyleFromGameSettingsService()
    {
        // Arrange
        const DiceStyle expectedDieStyle = DiceStyle.Red;

        _gameSettingsService.DieStyle.Returns(expectedDieStyle);

        // Act
        var dieStyle = _sut.DieStyle;

        // Assert
        dieStyle.ShouldBe(expectedDieStyle);
    }
    
    [Fact]
    public void SettingsSpeedCaption_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSettingsSpeedCaption = "SettingsSpeedCaptionText";

        _localizationService.GetString("SettingsSpeedCaptionText").Returns(expectedSettingsSpeedCaption);

        // Act
        var settingsSpeedCaption = _sut.SettingsSpeedCaption;

        // Assert
        settingsSpeedCaption.ShouldBe(expectedSettingsSpeedCaption);
    }

    [Fact]
    public void IsStyleBlue_ShouldReturnTrue_WhenDieStyleIsBlue()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Blue);

        // Act
        var isStyleBlue = _sut.IsStyleBlue;

        // Assert
        isStyleBlue.ShouldBeTrue();
    }

    [Fact]
    public void IsStyleBlue_ShouldReturnFalse_WhenDieStyleIsNotBlue()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Red);

        // Act
        var isStyleBlue = _sut.IsStyleBlue;

        // Assert
        isStyleBlue.ShouldBeFalse();
    }

    [Fact]
    public void IsStyleRed_ShouldReturnTrue_WhenDieStyleIsRed()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Red);

        // Act
        var isStyleRed = _sut.IsStyleRed;

        // Assert
        isStyleRed.ShouldBeTrue();
    }

    [Fact]
    public void IsStyleRed_ShouldReturnFalse_WhenDieStyleIsNotRed()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Blue);

        // Act
        var isStyleRed = _sut.IsStyleRed;

        // Assert
        isStyleRed.ShouldBeFalse();
    }

    [Fact]
    public void IsStyleWhite_ShouldReturnTrue_WhenDieStyleIsClassic()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Classic);

        // Act
        var isStyleWhite = _sut.IsStyleWhite;

        // Assert
        isStyleWhite.ShouldBeTrue();
    }

    [Fact]
    public void IsStyleWhite_ShouldReturnFalse_WhenDieStyleIsNotClassic()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Blue);

        // Act
        var isStyleWhite = _sut.IsStyleWhite;

        // Assert
        isStyleWhite.ShouldBeFalse();
    }

    [Fact]
    public void IsSpeedVerySlow_ShouldReturnTrue_WhenDieSpeedIsVerySlow()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.VerySlow);

        // Act
        var isSpeedVerySlow = _sut.IsSpeedVerySlow;

        // Assert
        isSpeedVerySlow.ShouldBeTrue();
    }

    [Fact]
    public void IsSpeedVerySlow_ShouldReturnFalse_WhenDieSpeedIsNotVerySlow()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.Slow);

        // Act
        var isSpeedVerySlow = _sut.IsSpeedVerySlow;

        // Assert
        isSpeedVerySlow.ShouldBeFalse();
    }

    [Fact]
    public void IsSpeedSlow_ShouldReturnTrue_WhenDieSpeedIsSlow()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.Slow);

        // Act
        var isSpeedSlow = _sut.IsSpeedSlow;

        // Assert
        isSpeedSlow.ShouldBeTrue();
    }

    [Fact]
    public void IsSpeedSlow_ShouldReturnFalse_WhenDieSpeedIsNotSlow()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.VerySlow);

        // Act
        var isSpeedSlow = _sut.IsSpeedSlow;

        // Assert
        isSpeedSlow.ShouldBeFalse();
    }

    [Fact]
    public void IsSpeedFast_ShouldReturnTrue_WhenDieSpeedIsFast()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.Fast);

        // Act
        var isSpeedFast = _sut.IsSpeedFast;

        // Assert
        isSpeedFast.ShouldBeTrue();
    }

    [Fact]
    public void IsSpeedFast_ShouldReturnFalse_WhenDieSpeedIsNotFast()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.Slow);

        // Act
        var isSpeedFast = _sut.IsSpeedFast;

        // Assert
        isSpeedFast.ShouldBeFalse();
    }

    [Fact]
    public void IsSpeedVeryFast_ShouldReturnTrue_WhenDieSpeedIsVeryFast()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.VeryFast);

        // Act
        var isSpeedVeryFast = _sut.IsSpeedVeryFast;

        // Assert
        isSpeedVeryFast.ShouldBeTrue();
    }

    [Fact]
    public void IsSpeedVeryFast_ShouldReturnFalse_WhenDieSpeedIsNot15()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns(30);

        // Act
        var isSpeedVeryFast = _sut.IsSpeedVeryFast;

        // Assert
        isSpeedVeryFast.ShouldBeFalse();
    }

    [Fact]
    public void IsAngLow_ShouldReturnTrue_WhenDieAngleIs0()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(0);

        // Act
        var isAngLow = _sut.IsAngleLow;

        // Assert
        isAngLow.ShouldBeTrue();
    }

    [Fact]
    public void IsAngLow_ShouldReturnFalse_WhenDieAngleIsNot0()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(2);

        // Act
        var isAngLow = _sut.IsAngleLow;

        // Assert
        isAngLow.ShouldBeFalse();
    }

    [Fact]
    public void IsAngHigh_ShouldReturnTrue_WhenDieAngleIs2()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(2);

        // Act
        var isAngHigh = _sut.IsAngleHigh;

        // Assert
        isAngHigh.ShouldBeTrue();
    }

    [Fact]
    public void IsAngHigh_ShouldReturnFalse_WhenDieAngleIsNot2()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(0);

        // Act
        var isAngHigh = _sut.IsAngleHigh;

        // Assert
        isAngHigh.ShouldBeFalse();
    }

    [Fact]
    public void IsAngVeryHigh_ShouldReturnTrue_WhenDieAngleIs4()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(4);

        // Act
        var isAngVeryHigh = _sut.IsAngleVeryHigh;

        // Assert
        isAngVeryHigh.ShouldBeTrue();
    }

    [Fact]
    public void IsAngVeryHigh_ShouldReturnFalse_WhenDieAngleIsNot4()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(2);

        // Act
        var isAngVeryHigh = _sut.IsAngleVeryHigh;

        // Assert
        isAngVeryHigh.ShouldBeFalse();
    }
    
    [Fact]
    public void SelectedLanguage_ShouldReturnAvailableLanguage_MatchingActiveLanguageCode()
    {
        // Arrange
        var language  = new Language("en",false); // Example language code
        _localizationService.ActiveLanguage.Returns(language);

        // Act
        var selectedLanguage = _sut.SelectedLanguage;

        // Assert
        selectedLanguage.Code.ShouldBe("en");
    }
    
    [Fact]
    public void SelectedLanguage_ShouldCallSetActiveLanguage_WithCorrectArgument()
    {
        // Arrange
        var expectedLanguage = new Language( "en",true,"english"); // Example language code

        // Act
        _sut.SelectedLanguage = expectedLanguage;

        // Assert
        _localizationService.Received(1).SetActiveLanguage(expectedLanguage);
    }

    [Fact]
    public void AvailableLanguages_ReturnsList_FromLocalizationService()
    {
        _sut.AvailableLanguages.ShouldBe(new List<Language>() { _defLanguage });
    }

    #region hub management

    [Fact]
    public void HubSectionTitle_ShouldReturnCorrectLocalizedString()
    {
        _localizationService.GetString("HubSectionTitleText").Returns("Relay Hub");

        _sut.HubSectionTitle.ShouldBe("Relay Hub");
    }

    [Fact]
    public void HubSelectLabel_ShouldReturnCorrectLocalizedString()
    {
        _localizationService.GetString("SelectHubLabel").Returns("Active hub");

        _sut.HubSelectLabel.ShouldBe("Active hub");
    }

    [Fact]
    public void HubAddHubLabel_ShouldReturnCorrectLocalizedString()
    {
        _localizationService.GetString("AddHubLabel").Returns("Add Hub");

        _sut.HubAddHubLabel.ShouldBe("Add Hub");
    }

    [Fact]
    public async Task AttachHandlers_ShouldLoadHubs_BookmarkRowsAndRestoreActiveSelection_WithoutCallingSelectHub()
    {
        SetupProviderHubs([DemoHub, CustomHub], "default");

        _sut.AttachHandlers();
        await WaitFor(() => _sut.Hubs.Count == 2);

        _sut.Hubs.Count.ShouldBe(2);
        _sut.Hubs[0].Id.ShouldBe("default");
        _sut.Hubs[0].IsBuiltIn.ShouldBeTrue();
        _sut.Hubs[0].CanEdit.ShouldBeFalse();
        _sut.Hubs[0].CanRemove.ShouldBeFalse();
        _sut.Hubs[1].Id.ShouldBe("custom-1");
        _sut.Hubs[1].CanEdit.ShouldBeTrue();
        _sut.Hubs[1].CanRemove.ShouldBeTrue();

        _sut.SelectedHub.ShouldNotBeNull();
        _sut.SelectedHub!.Id.ShouldBe("default");
        await _hubConfigurationProvider.DidNotReceive().SelectHub(Arg.Any<string>());
    }

    [Fact]
    public async Task SelectedHub_WhenChanged_ShouldCallSelectHub()
    {
        SetupProviderHubs([DemoHub, CustomHub], "default");
        _sut.AttachHandlers();
        await WaitFor(() => _sut.Hubs.Count == 2);

        _sut.SelectedHub = _sut.Hubs.First(h => h.Id == "custom-1");
        await WaitFor(() => _hubConfigurationProvider.ReceivedCalls()
            .Any(c => c.GetMethodInfo().Name == nameof(IRelayHubConfigurationProvider.SelectHub)));

        _hubConfigurationProvider.Received(1).SelectHub("custom-1");
    }

    [Fact]
    public async Task SelectedHub_WhenEarlierSelectionFails_LaterSelectionStillRuns()
    {
        SetupProviderHubs([DemoHub, CustomHub], "default");
        _sut.AttachHandlers();
        await WaitFor(() => _sut.Hubs.Count == 2);

        _hubConfigurationProvider.SelectHub("custom-1")
            .Returns(Task.FromException(new Exception("selection failed")));
        _hubConfigurationProvider.SelectHub("default").Returns(Task.CompletedTask);

        _sut.SelectedHub = _sut.Hubs.First(h => h.Id == "custom-1");
        _sut.SelectedHub = _sut.Hubs.First(h => h.Id == "default");

        await WaitFor(() => _hubConfigurationProvider.ReceivedCalls().Any(c =>
            c.GetMethodInfo().Name == nameof(IRelayHubConfigurationProvider.SelectHub)
            && c.GetArguments()[0] as string == "default"));

        _hubConfigurationProvider.Received(1).SelectHub("default");
    }

    [Fact]
    public async Task AddHubCommand_WhenExecuted_ShouldShowAddHubDialog()
    {
        var navigationService = Substitute.For<INavigationService>();
        _sut.SetNavigationService(navigationService);

        await ((IAsyncCommand)_sut.AddHubCommand).ExecuteAsync();

        navigationService.Received(1).ShowViewModelForResultAsync<AddHubViewModel, AddHubResult?>(Arg.Any<AddHubViewModel>());
    }

    [Fact]
    public async Task AddHub_WhenCancelled_ShouldNotAddHub()
    {
        var navigationService = Substitute.For<INavigationService>();
        SetupProviderHubs([DemoHub], "default");
        _sut.SetNavigationService(navigationService);
        navigationService.ShowViewModelForResultAsync<AddHubViewModel, AddHubResult?>(Arg.Any<AddHubViewModel>())
            .Returns(Task.FromResult<AddHubResult?>(null));

        await ((IAsyncCommand)_sut.AddHubCommand).ExecuteAsync();

        await _hubConfigurationProvider.DidNotReceive().AddHub(Arg.Any<HubConfigData>());
    }

    [Fact]
    public async Task AddHub_WhenConfirmed_ShouldAddUserHub_ThenReload()
    {
        var navigationService = Substitute.For<INavigationService>();
        var reloadTask = new TaskCompletionSource();
        SetupProviderHubs([DemoHub], "default");
        _sut.SetNavigationService(navigationService);
        navigationService.ShowViewModelForResultAsync<AddHubViewModel, AddHubResult?>(Arg.Any<AddHubViewModel>())
            .Returns(Task.FromResult<AddHubResult?>(
                new AddHubResult { Name = "My Hub", BaseUrl = "http://my-hub.example", ApiKey = "secret" }));
        _hubConfigurationProvider
            .When(provider => provider.AddHub(Arg.Any<HubConfigData>()))
            .Do(callInfo =>
            {
                _hubConfigurationProvider.GetHubs().Returns(Task.FromResult<IReadOnlyList<HubConfigData>>(
                    new[] { DemoHub, callInfo.Arg<HubConfigData>() }));
                reloadTask.TrySetResult();
            });

        await ((IAsyncCommand)_sut.AddHubCommand).ExecuteAsync();
        await reloadTask.Task;

        await _hubConfigurationProvider.Received(1).AddHub(Arg.Is<HubConfigData>(h =>
            HasNewGuidId(h)
            && h.Name == "My Hub"
            && h.BaseUrl == "http://my-hub.example"
            && h.ApiKey == "secret"
            && !h.IsBuiltIn));
        await WaitFor(() => _sut.Hubs.Count == 2);
        var addedRow = _sut.Hubs.First(h => h.Id != "default");
        addedRow.Name.ShouldBe("My Hub");
        addedRow.CanEdit.ShouldBeTrue();
        addedRow.CanRemove.ShouldBeTrue();
    }

    [Fact]
    public async Task RemoveHubCommand_WhenExecuted_ShouldRemoveHub()
    {
        SetupProviderHubs([DemoHub, CustomHub], "default");
        _sut.AttachHandlers();
        await WaitFor(() => _sut.Hubs.Count == 2);
        var entry = _sut.Hubs.First(h => h.Id == "custom-1");

        await ((IAsyncCommand<HubEntryViewModel>)_sut.RemoveHubCommand).ExecuteAsync(entry);

        _hubConfigurationProvider.Received(1).RemoveHub("custom-1");
    }

    [Fact]
    public async Task RemoveHubCommand_WhenExecutedWithNull_ShouldNotRemove()
    {
        await ((IAsyncCommand<HubEntryViewModel>)_sut.RemoveHubCommand).ExecuteAsync(null!);

        _hubConfigurationProvider.DidNotReceive().RemoveHub(Arg.Any<string>());
    }

    [Fact]
    public async Task RemoveHubCommand_WhenBuiltIn_ShouldNotRemove()
    {
        SetupProviderHubs([DemoHub], "default");
        _sut.AttachHandlers();
        await WaitFor(() => _sut.Hubs.Count == 1);
        var entry = _sut.Hubs.Single();

        await ((IAsyncCommand<HubEntryViewModel>)_sut.RemoveHubCommand).ExecuteAsync(entry);

        _hubConfigurationProvider.DidNotReceive().RemoveHub(Arg.Any<string>());
    }

    [Fact]
    public async Task AttachHandlers_ShouldProbeStatusForEachHub_WithRowOptions()
    {
        SetupProviderHubs([DemoHub, CustomHub], "default");
        _relayRoomClient.Health(Arg.Any<CancellationToken>(), Arg.Any<RelayClientOptions>())
            .Returns((RelayClientError?)null);

        _sut.AttachHandlers();
        await WaitFor(() => _sut.Hubs.Count == 2);

        await _relayRoomClient.Received(2).Health(Arg.Any<CancellationToken>(), Arg.Any<RelayClientOptions>());
        _relayRoomClient.Received(1).Health(
            Arg.Any<CancellationToken>(),
            Arg.Is<RelayClientOptions>(o => o.BaseUrl == CustomHub.BaseUrl && o.ApiKey == CustomHub.ApiKey));
        await WaitFor(() => _sut.Hubs.All(h => h.Status == HubStatus.Online));
    }

    [Fact]
    public async Task Health_WhenErrorReturned_ShouldMapToOffline()
    {
        SetupProviderHubs([CustomHub], "custom-1");
        _relayRoomClient.Health(Arg.Any<CancellationToken>(), Arg.Any<RelayClientOptions>())
            .Returns(Task.FromResult<RelayClientError?>(
                new RelayClientError(RelayClientErrorCode.NetworkError, "unreachable")));

        _sut.AttachHandlers();
        await WaitFor(() => _sut.Hubs.Count == 1);

        await WaitFor(() => _sut.Hubs.Single().Status == HubStatus.Offline);
    }

    [Fact]
    public async Task Health_WhenProbeThrows_ShouldMapToOffline()
    {
        SetupProviderHubs([CustomHub], "custom-1");
        _relayRoomClient.Health(Arg.Any<CancellationToken>(), Arg.Any<RelayClientOptions>())
            .Returns(Task.FromException<RelayClientError?>(new Exception("probe failed")));

        _sut.AttachHandlers();
        await WaitFor(() => _sut.Hubs.Count == 1);

        await WaitFor(() => _sut.Hubs.Single().Status == HubStatus.Offline);
    }

    [Fact]
    public async Task DetachHandlers_ShouldCancelHealthProbes()
    {
        var healthGate = new TaskCompletionSource<RelayClientError?>();
        _relayRoomClient.Health(Arg.Any<CancellationToken>(), Arg.Any<RelayClientOptions>())
            .Returns(callInfo =>
            {
                var cancellationToken = callInfo.Arg<CancellationToken>();
                cancellationToken.Register(() => healthGate.TrySetCanceled(cancellationToken));
                return healthGate.Task;
            });
        SetupProviderHubs([CustomHub], "custom-1");

        _sut.AttachHandlers();
        await WaitFor(() => _sut.Hubs.Count == 1);
        _sut.Hubs.Single().Status.ShouldBe(HubStatus.Checking);

        _sut.DetachHandlers();

        await WaitFor(() => _sut.Hubs.Single().Status == HubStatus.Unknown);
        _sut.Hubs.Single().IsCheckingStatus.ShouldBeFalse();
    }

    [Fact]
    public async Task HubSave_WhenDetachedWhileReloadInFlight_DoesNotThrowAndKeepsHubs()
    {
        SetupProviderHubs([DemoHub, CustomHub], "default");
        _sut.AttachHandlers();
        await WaitFor(() => _sut.Hubs.Count == 2);

        var reloadGate = new TaskCompletionSource<IReadOnlyList<HubConfigData>>();
        _hubConfigurationProvider.GetHubs().Returns(reloadGate.Task);

        var entry = _sut.Hubs.First(h => h.Id == "custom-1");
        await entry.StartEditing();
        var saveTask = ((IAsyncCommand)entry.SaveCommand).ExecuteAsync();

        await WaitFor(() => _hubConfigurationProvider.ReceivedCalls()
            .Any(c => c.GetMethodInfo().Name == nameof(IRelayHubConfigurationProvider.UpdateHub)));

        _sut.DetachHandlers();
        reloadGate.SetResult([DemoHub, CustomHub]);
        await saveTask;

        _sut.Hubs.Count.ShouldBe(2);
    }

    #endregion
}