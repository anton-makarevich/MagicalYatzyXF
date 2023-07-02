using System;
using System.Globalization;
using System.IO;
using FluentAssertions;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Services.Localization;
using Xunit;

namespace MagicalYatzyTests.Services.Localization;

public class GlobalizationInvariantLocalizationServiceTests
{
    private readonly GlobalizationInvariantLocalizationService _sut;

    public GlobalizationInvariantLocalizationServiceTests()
    {
        _sut = new GlobalizationInvariantLocalizationService();
    }
    
    [Fact]
    public void DefaultLanguage_IsSet_When_ServiceIsCreated()
    {
        _sut.Language.Should().Be(LanguageCode.Default);
    }
    
    [Fact]
    public void SetSystemCulture_WithLanguage_Should_SetLanguage() 
    {
        const LanguageCode languageCode = LanguageCode.BeBy;
        _sut.SetSystemCulture(languageCode);
        _sut.Language.Should().Be(languageCode);
    }
    
    [Fact]
    public void DefaultLanguage_IsEnglish() 
    {
        const LanguageCode languageCode = LanguageCode.Default;
        _sut.SetSystemCulture(languageCode);
        _sut.Language.ToCultureString().Should().Be("en-US");
    }
    
    [Fact]
    public void SetSystemCulture_WithCulture_Should_ThrowException() 
    {
        FluentActions.Invoking(() => _sut.SetSystemCulture(CultureInfo.CurrentCulture))
            .Should().Throw<NotSupportedException>();
    }
    
    [Fact]
    public void SetSystemCulture__Should_ThrowException_When_CultureIsNotSupported() 
    {
        FluentActions.Invoking(() => _sut.SetSystemCulture(LanguageCode.EnUs))
            .Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void GetLocalizedString_Should_ReturnLocalizedValue_When_ItExistsForSelectedLanguage()
    {
        _sut.SetSystemCulture(LanguageCode.BeBy);
        var localizedString = _sut.GetLocalizedString("PlayerNameDefault");
        localizedString.Should().Be("Гулец");
    }
    
    [Fact]
    public void GetLocalizedString_Should_ReturnDefaultLocalizedValue_When_ItDoesNotExistForSelectedLanguage()
    {
        _sut.SetSystemCulture(LanguageCode.BeBy);
        var localizedString = _sut.GetLocalizedString("OtherAllWriteContent");
        localizedString.Should().Be("Learning English and Russian writing with Sanet AllWrite");
    }
}