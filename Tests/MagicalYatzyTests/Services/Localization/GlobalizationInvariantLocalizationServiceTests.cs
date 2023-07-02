using System;
using System.Globalization;
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
    public void SetSystemCulture_WithALanguage_Should_SetLanguage() 
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
    
}