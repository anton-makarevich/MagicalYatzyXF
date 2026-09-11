using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Shouldly;
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
        var defaultLanguage = _sut.Languages.FirstOrDefault(l=>l.IsDefault);
        _sut.ActiveLanguage.ShouldBe(defaultLanguage);
    }
    
    [Fact]
    public void SetActiveLanguage_WithLanguage_Should_SetLanguage() 
    {
        var language = _sut.Languages.FirstOrDefault(l=>l.Code =="be");
        _sut.SetActiveLanguage(language);
        _sut.ActiveLanguage.ShouldBe(language);
    }
    
    [Fact]
    public void SetActiveLanguage_WithLanguageCode_Should_SetLanguage() 
    {
        const string languageCode = "be";
        _sut.SetActiveLanguage(languageCode);
        _sut.ActiveLanguage.Code.ShouldBe(languageCode);
    }
    
    [Fact]
    public void SetSystemCulture_WithUnavailableLanguage_ShouldThrowException() 
    {
        var language = new Language("agr", false);
        Should.Throw<FileNotFoundException>(() => _sut.SetActiveLanguage(language));
    }
    
    [Fact]
    public void DefaultLanguage_IsEnglish() 
    {
        _sut.ActiveLanguage.Code.ShouldBe("en");
    }
    
    [Fact]
    public void SetSystemCulture_WithCulture_Should_ThrowException() 
    {
        Should.Throw<NotSupportedException>(() => _sut.SetActiveLanguage(CultureInfo.CurrentCulture));
    }

    [Fact]
    public void GetLocalizedString_Should_ReturnLocalizedValue_When_ItExistsForSelectedLanguage()
    {
        _sut.SetActiveLanguage(new Language("be", false));
        var localizedString = _sut.GetLocalizedString("PlayerNameDefault");
        localizedString.ShouldBe("Гулец");
    }
    
    [Fact]
    public void GetLocalizedString_Should_ReturnDefaultLocalizedValue_When_ItDoesNotExistForSelectedLanguage()
    {
        _sut.SetActiveLanguage(new Language("be", false));
        var localizedString = _sut.GetLocalizedString("OtherAllWriteContent");
        localizedString.ShouldBe("Learning English and Russian writing with Sanet AllWrite");
    }

    [Fact]
    public void Languages_ReturnsAllAvailableLanguages()
    {
        var expectedLanguages = new List<Language>();
        
        var languages = _sut.Languages;

        foreach (var language in expectedLanguages)
        {
            languages.ShouldContain(language);
        }
    }
}