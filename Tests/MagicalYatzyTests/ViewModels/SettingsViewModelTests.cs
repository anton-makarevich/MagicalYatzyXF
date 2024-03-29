﻿using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using Sanet.MagicalYatzy.Models;
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
    
    private readonly Language _defLanguage = new Language("en", true, "english");

    public SettingsViewModelTests()
    {
        var dicePanel = Substitute.For<IDicePanel>();
        _gameSettingsService = Substitute.For<IGameSettingsService>();
        _localizationService = Substitute.For<ILocalizationService>();

        _localizationService.Languages.Returns(new List<Language> {_defLanguage});

        _sut = new SettingsViewModel(dicePanel, _gameSettingsService, _localizationService);
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
    public void LanguageLabel_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expected = "Language";

        _localizationService.GetLocalizedString("LanguageLabel").Returns(expected);

        // Act
        var language = _sut.LanguageLabel;

        // Assert
        language.Should().Be(expected);
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
    public void IsSpeedVerySlow_ShouldSetDieSpeedToVerySlow_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedVerySlow = true;

        // Assert
        _gameSettingsService.DieSpeed.Should().Be((int)DiceSpeed.VerySlow);
    }

    [Fact]
    public void IsSpeedSlow_ShouldSetDieSpeedToSlow_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedSlow = true;

        // Assert
        _gameSettingsService.DieSpeed.Should().Be((int)DiceSpeed.Slow);
    }

    [Fact]
    public void IsSpeedFast_ShouldSetDieSpeedToFast_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedFast = true;

        // Assert
        _gameSettingsService.DieSpeed.Should().Be((int)DiceSpeed.Fast);
    }

    [Fact]
    public void IsSpeedVeryFast_ShouldSetDieSpeedToVeryFast_WhenValueIsTrue()
    {
        // Act
        _sut.IsSpeedVeryFast = true;

        // Assert
        _gameSettingsService.DieSpeed.Should().Be((int)DiceSpeed.VeryFast);
    }

    [Fact]
    public void IsAngLow_ShouldSetDieAngleTo0_WhenValueIsTrue()
    {
        // Act
        _sut.IsAngleLow = true;

        // Assert
        _gameSettingsService.DieAngle.Should().Be(0);
    }

    [Fact]
    public void IsAngHigh_ShouldSetDieAngleTo2_WhenValueIsTrue()
    {
        // Act
        _sut.IsAngleHigh = true;

        // Assert
        _gameSettingsService.DieAngle.Should().Be(2);
    }

    [Fact]
    public void IsAngVeryHigh_ShouldSetDieAngleTo4_WhenValueIsTrue()
    {
        // Act
        _sut.IsAngleVeryHigh = true;

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
    
    [Fact]
    public void SettingsStyleCaption_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSettingsStyleCaption = "SettingsStyleCaptionText";

        _localizationService.GetLocalizedString("SettingsStyleCaptionText").Returns(expectedSettingsStyleCaption);

        // Act
        var settingsStyleCaption = _sut.SettingsStyleCaption;

        // Assert
        settingsStyleCaption.Should().Be(expectedSettingsStyleCaption);
    }

    [Fact]
    public void AngLowText_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedAngLowText = "AngLowText";

        _localizationService.GetLocalizedString("AngLowText").Returns(expectedAngLowText);

        // Act
        var angLowText = _sut.AngleLowText;

        // Assert
        angLowText.Should().Be(expectedAngLowText);
    }

    [Fact]
    public void AngHighText_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedAngHighText = "AngHighText";

        _localizationService.GetLocalizedString("AngHighText").Returns(expectedAngHighText);

        // Act
        var angHighText = _sut.AngleHighText;

        // Assert
        angHighText.Should().Be(expectedAngHighText);
    }

    [Fact]
    public void AngVeryHighText_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedAngVeryHighText = "AngVeryHighText";

        _localizationService.GetLocalizedString("AngVeryHighText").Returns(expectedAngVeryHighText);

        // Act
        var angVeryHighText = _sut.AngleVeryHighText;

        // Assert
        angVeryHighText.Should().Be(expectedAngVeryHighText);
    }

    [Fact]
    public void SettingsAngleCaption_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSettingsAngleCaption = "SettingsAngleCaptionText";

        _localizationService.GetLocalizedString("SettingsAngleCaptionText").Returns(expectedSettingsAngleCaption);

        // Act
        var settingsAngleCaption = _sut.SettingsAngleCaption;

        // Assert
        settingsAngleCaption.Should().Be(expectedSettingsAngleCaption);
    }

    [Fact]
    public void SpeedSlow_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSpeedSlow = "SpeedSlowText";

        _localizationService.GetLocalizedString("SpeedSlowText").Returns(expectedSpeedSlow);

        // Act
        var speedSlow = _sut.SpeedSlow;

        // Assert
        speedSlow.Should().Be(expectedSpeedSlow);
    }

    [Fact]
    public void SpeedVerySlow_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSpeedVerySlow = "SpeedVerySlowText";

        _localizationService.GetLocalizedString("SpeedVerySlowText").Returns(expectedSpeedVerySlow);

        // Act
        var speedVerySlow = _sut.SpeedVerySlow;

        // Assert
        speedVerySlow.Should().Be(expectedSpeedVerySlow);
    }

    [Fact]
    public void SpeedFast_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSpeedFast = "SpeedFastText";

        _localizationService.GetLocalizedString("SpeedFastText").Returns(expectedSpeedFast);

        // Act
        var speedFast = _sut.SpeedFast;

        // Assert
        speedFast.Should().Be(expectedSpeedFast);
    }

    [Fact]
    public void SpeedVeryFast_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSpeedVeryFast = "SpeedVeryFastText";

        _localizationService.GetLocalizedString("SpeedVeryFastText").Returns(expectedSpeedVeryFast);

        // Act
        var speedVeryFast = _sut.SpeedVeryFast;

        // Assert
        speedVeryFast.Should().Be(expectedSpeedVeryFast);
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
        dieAngle.Should().Be(expectedDieAngle);
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
        dieSpeed.Should().Be((DiceSpeed)expectedDieSpeed);
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
        dieStyle.Should().Be(expectedDieStyle);
    }
    
    
    [Fact]
    public void SettingsSpeedCaption_ShouldReturnCorrectLocalizedString()
    {
        // Arrange
        const string expectedSettingsSpeedCaption = "SettingsSpeedCaptionText";

        _localizationService.GetLocalizedString("SettingsSpeedCaptionText").Returns(expectedSettingsSpeedCaption);

        // Act
        var settingsSpeedCaption = _sut.SettingsSpeedCaption;

        // Assert
        settingsSpeedCaption.Should().Be(expectedSettingsSpeedCaption);
    }

    [Fact]
    public void IsStyleBlue_ShouldReturnTrue_WhenDieStyleIsBlue()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Blue);

        // Act
        var isStyleBlue = _sut.IsStyleBlue;

        // Assert
        isStyleBlue.Should().BeTrue();
    }

    [Fact]
    public void IsStyleBlue_ShouldReturnFalse_WhenDieStyleIsNotBlue()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Red);

        // Act
        var isStyleBlue = _sut.IsStyleBlue;

        // Assert
        isStyleBlue.Should().BeFalse();
    }

    [Fact]
    public void IsStyleRed_ShouldReturnTrue_WhenDieStyleIsRed()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Red);

        // Act
        var isStyleRed = _sut.IsStyleRed;

        // Assert
        isStyleRed.Should().BeTrue();
    }

    [Fact]
    public void IsStyleRed_ShouldReturnFalse_WhenDieStyleIsNotRed()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Blue);

        // Act
        var isStyleRed = _sut.IsStyleRed;

        // Assert
        isStyleRed.Should().BeFalse();
    }

    [Fact]
    public void IsStyleWhite_ShouldReturnTrue_WhenDieStyleIsClassic()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Classic);

        // Act
        var isStyleWhite = _sut.IsStyleWhite;

        // Assert
        isStyleWhite.Should().BeTrue();
    }

    [Fact]
    public void IsStyleWhite_ShouldReturnFalse_WhenDieStyleIsNotClassic()
    {
        // Arrange
        _gameSettingsService.DieStyle.Returns(DiceStyle.Blue);

        // Act
        var isStyleWhite = _sut.IsStyleWhite;

        // Assert
        isStyleWhite.Should().BeFalse();
    }

    [Fact]
    public void IsSpeedVerySlow_ShouldReturnTrue_WhenDieSpeedIsVerySlow()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.VerySlow);

        // Act
        var isSpeedVerySlow = _sut.IsSpeedVerySlow;

        // Assert
        isSpeedVerySlow.Should().BeTrue();
    }

    [Fact]
    public void IsSpeedVerySlow_ShouldReturnFalse_WhenDieSpeedIsNotVerySlow()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.Slow);

        // Act
        var isSpeedVerySlow = _sut.IsSpeedVerySlow;

        // Assert
        isSpeedVerySlow.Should().BeFalse();
    }

    [Fact]
    public void IsSpeedSlow_ShouldReturnTrue_WhenDieSpeedIsSlow()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.Slow);

        // Act
        var isSpeedSlow = _sut.IsSpeedSlow;

        // Assert
        isSpeedSlow.Should().BeTrue();
    }

    [Fact]
    public void IsSpeedSlow_ShouldReturnFalse_WhenDieSpeedIsNotSlow()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.VerySlow);

        // Act
        var isSpeedSlow = _sut.IsSpeedSlow;

        // Assert
        isSpeedSlow.Should().BeFalse();
    }

    [Fact]
    public void IsSpeedFast_ShouldReturnTrue_WhenDieSpeedIsFast()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.Fast);

        // Act
        var isSpeedFast = _sut.IsSpeedFast;

        // Assert
        isSpeedFast.Should().BeTrue();
    }

    [Fact]
    public void IsSpeedFast_ShouldReturnFalse_WhenDieSpeedIsNotFast()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.Slow);

        // Act
        var isSpeedFast = _sut.IsSpeedFast;

        // Assert
        isSpeedFast.Should().BeFalse();
    }

    [Fact]
    public void IsSpeedVeryFast_ShouldReturnTrue_WhenDieSpeedIsVeryFast()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns((int)DiceSpeed.VeryFast);

        // Act
        var isSpeedVeryFast = _sut.IsSpeedVeryFast;

        // Assert
        isSpeedVeryFast.Should().BeTrue();
    }

    [Fact]
    public void IsSpeedVeryFast_ShouldReturnFalse_WhenDieSpeedIsNot15()
    {
        // Arrange
        _gameSettingsService.DieSpeed.Returns(30);

        // Act
        var isSpeedVeryFast = _sut.IsSpeedVeryFast;

        // Assert
        isSpeedVeryFast.Should().BeFalse();
    }

    [Fact]
    public void IsAngLow_ShouldReturnTrue_WhenDieAngleIs0()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(0);

        // Act
        var isAngLow = _sut.IsAngleLow;

        // Assert
        isAngLow.Should().BeTrue();
    }

    [Fact]
    public void IsAngLow_ShouldReturnFalse_WhenDieAngleIsNot0()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(2);

        // Act
        var isAngLow = _sut.IsAngleLow;

        // Assert
        isAngLow.Should().BeFalse();
    }

    [Fact]
    public void IsAngHigh_ShouldReturnTrue_WhenDieAngleIs2()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(2);

        // Act
        var isAngHigh = _sut.IsAngleHigh;

        // Assert
        isAngHigh.Should().BeTrue();
    }

    [Fact]
    public void IsAngHigh_ShouldReturnFalse_WhenDieAngleIsNot2()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(0);

        // Act
        var isAngHigh = _sut.IsAngleHigh;

        // Assert
        isAngHigh.Should().BeFalse();
    }

    [Fact]
    public void IsAngVeryHigh_ShouldReturnTrue_WhenDieAngleIs4()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(4);

        // Act
        var isAngVeryHigh = _sut.IsAngleVeryHigh;

        // Assert
        isAngVeryHigh.Should().BeTrue();
    }

    [Fact]
    public void IsAngVeryHigh_ShouldReturnFalse_WhenDieAngleIsNot4()
    {
        // Arrange
        _gameSettingsService.DieAngle.Returns(2);

        // Act
        var isAngVeryHigh = _sut.IsAngleVeryHigh;

        // Assert
        isAngVeryHigh.Should().BeFalse();
    }
    
    [Fact]
    public void SelectedLanguage_ShouldReturnActiveLanguageCode_FromLocalizationService()
    {
        // Arrange
        var language  = new Language("en",false); // Example language code
        _localizationService.ActiveLanguage.Returns(language);

        // Act
        var selectedLanguage = _sut.SelectedLanguage;

        // Assert
        selectedLanguage.Should().Be(language);
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
        _sut.AvailableLanguages.Should().Equal(new List<Language>() { _defLanguage });
    }
}