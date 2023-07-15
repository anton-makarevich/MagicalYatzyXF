using FluentAssertions;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.ViewModels;
using Xunit;

namespace MagicalYatzyTests.ViewModels;

public class SettingsViewModelTests
{
    private readonly SettingsViewModel _sut;
    private readonly IGameSettingsService _gameSettingsService;
    private readonly ILocalizationService _localizationService;

    public SettingsViewModelTests()
    {
        _gameSettingsService = Substitute.For<IGameSettingsService>();
        _localizationService = Substitute.For<ILocalizationService>();

        _sut = new SettingsViewModel(_gameSettingsService, _localizationService);
    }

    [Fact]
    public void Title_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedTitle = "SettingsCaptionText";

        _localizationService.GetLocalizedString("SettingsCaptionText").Returns(expectedTitle);

        // Act
        var title = _sut.Title;

        // Assert
        title.Should().Be(expectedTitle);
    }

    [Fact]
    public void IsStyleBlue_ShouldSetDieStyleToBlue_WhenValueIsTrue()
    {
        // Act
        _sut.IsStyleBlue = true;

        // Assert
        _gameSettingsService.DieStyle.Should().Be(DiceStyle.Blue);
    }

    [Fact]
    public void IsStyleRed_ShouldSetDieStyleToRed_WhenValueIsTrue()
    {
        // Act
        _sut.IsStyleRed = true;

        // Assert
        _gameSettingsService.DieStyle.Should().Be(DiceStyle.Red);
    }

    [Fact]
    public void IsStyleWhite_ShouldSetDieStyleToClassic_WhenValueIsTrue()
    {
        // Act
        _sut.IsStyleWhite = true;

        // Assert
        _gameSettingsService.DieStyle.Should().Be(DiceStyle.Classic);
    }

        [Fact]
    public void IsSpeedVerySlow_ShouldSetDieSpeedTo70_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedVerySlow = true;

        // Assert
        _gameSettingsService.DieSpeed.Should().Be(70);
    }

    [Fact]
    public void IsSpeedSlow_ShouldSetDieSpeedTo50_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedSlow = true;

        // Assert
        _gameSettingsService.DieSpeed.Should().Be(50);
    }

    [Fact]
    public void IsSpeedFast_ShouldSetDieSpeedTo30_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedFast = true;

        // Assert
        _gameSettingsService.DieSpeed.Should().Be(30);
    }

    [Fact]
    public void IsSpeedVeryFast_ShouldSetDieSpeedTo15_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedVeryFast = true;

        // Assert
        _gameSettingsService.DieSpeed.Should().Be(15);
    }

    [Fact]
    public void IsAngLow_ShouldSetDieAngleTo0_WhenValueIsTrue()
    {
        // Act
        _sut.IsAngLow = true;

        // Assert
        _gameSettingsService.DieAngle.Should().Be(0);
    }

    [Fact]
    public void IsAngHigh_ShouldSetDieAngleTo2_WhenValueIsTrue()
    {
        // Act
        _sut.IsAngHigh = true;

        // Assert
        _gameSettingsService.DieAngle.Should().Be(2);
    }

    [Fact]
    public void IsAngVeryHigh_ShouldSetDieAngleTo4_WhenValueIsTrue()
    {
        // Act
        _sut.IsAngVeryHigh = true;

        // Assert
        _gameSettingsService.DieAngle.Should().Be(4);
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
        isSoundEnabled.Should().Be(expectedIsSoundEnabled);
    }

    [Fact]
    public void IsSoundEnabled_ShouldSetIsSoundEnabledInGameSettingsService_WhenValueChanged()
    {
        // Arrange
        const bool expectedIsSoundEnabled = true;

        // Act
        _sut.IsSoundEnabled = expectedIsSoundEnabled;

        // Assert
        _gameSettingsService.IsSoundEnabled.Should().Be(expectedIsSoundEnabled);
    }

    [Fact]
    public void SoundLabel_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSoundLabel = "SoundLabel";

        _localizationService.GetLocalizedString("SoundLabel").Returns(expectedSoundLabel);

        // Act
        var soundLabel = _sut.SoundLabel;

        // Assert
        soundLabel.Should().Be(expectedSoundLabel);
    }

    [Fact]
    public void OffContent_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedOffContent = "OffContent";

        _localizationService.GetLocalizedString("OffContent").Returns(expectedOffContent);

        // Act
        var offContent = _sut.OffContent;

        // Assert
        offContent.Should().Be(expectedOffContent);
    }

    [Fact]
    public void OnContent_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedOnContent = "OnContent";

        _localizationService.GetLocalizedString("OnContent").Returns(expectedOnContent);

        // Act
        var onContent = _sut.OnContent;

        // Assert
        onContent.Should().Be(expectedOnContent);
    }
}