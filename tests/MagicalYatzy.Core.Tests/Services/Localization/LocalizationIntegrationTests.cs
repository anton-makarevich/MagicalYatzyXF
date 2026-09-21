using System;
using System.Linq;
using System.Reflection;
using Shouldly;
using Sanet.Localization;
using Xunit;

namespace MagicalYatzyTests.Services.Localization;

public class LocalizationIntegrationTests
{
    private static ILocalizationService CreateSut()
    {
        var coreAssembly = Assembly.GetAssembly(typeof(Sanet.MagicalYatzy.ViewModels.Base.DicePanelViewModel))
                          ?? throw new InvalidOperationException("Core assembly not found");
        return new ResourceLocalizationService(
            coreAssembly,
            "Sanet.MagicalYatzy.Resources.Strings");
    }

    [Fact]
    public void GetString_ShouldResolveKnownAppKey_ThroughLibraryConfiguredWithAppResources()
    {
        var sut = CreateSut();

        var result = sut.GetString("PlayerNameDefault");

        result.ShouldBe("Player");
    }

    [Fact]
    public void DefaultLanguage_ShouldBeEnglish()
    {
        var sut = CreateSut();

        sut.ActiveLanguage.Code.ShouldBe("en");
    }

    [Fact]
    public void Languages_ShouldDiscoverAppLanguages_FromCoreResources()
    {
        var sut = CreateSut();

        sut.Languages.ShouldNotBeEmpty();
        sut.Languages.ShouldContain(l => l.Code == "be");
    }

    [Fact]
    public void SetActiveLanguage_ShouldSwitchLanguage_AndResolveBelarusianString()
    {
        var sut = CreateSut();

        sut.SetActiveLanguage("be");

        sut.ActiveLanguage.Code.ShouldBe("be");
        sut.GetString("PlayerNameDefault").ShouldBe("Гулец");
    }

    [Fact]
    public void GetString_ShouldFallBackToDefaultLanguage_WhenKeyMissingInActiveLanguage()
    {
        var sut = CreateSut();

        sut.SetActiveLanguage("be");

        sut.GetString("OtherAllWriteContent").ShouldBe(
            "Learning English and Russian writing with Sanet AllWrite");
    }

    [Fact]
    public void GetString_ShouldReturnRawKey_WhenKeyMissingInAllLanguages()
    {
        var sut = CreateSut();

        sut.GetString("DefinitelyMissingKey").ShouldBe("DefinitelyMissingKey");
    }

    [Fact]
    public void Languages_ShouldContainOnlyUniqueCodes()
    {
        var sut = CreateSut();

        sut.Languages.Select(l => l.Code).ShouldBeUnique();
    }
}
