using Sanet.MagicalYatzy.Services;
using Xunit;
using Sanet.MagicalYatzy.Resources;
using System.Globalization;
using Sanet.MagicalYatzy.Models;

namespace MagicalYatzyTests.ServiceTests
{
    public class LocalizationServiceTests
    {
        private readonly LocalizationService _sut = new LocalizationService();

        [Fact]
        public void SetsDefaultStringsCultureOnLoad()
        {
            Assert.Equal(CultureInfo.GetCultureInfo("en-US"), Strings.Culture);
        }

        [Fact]
        public void LoadsDefaultLanguage()
        {
            Assert.Equal(LanguageCode.Default, _sut.Language);
        }

        [Fact]
        public void SettingLanguageUpdatesLanguageAndCulture()
        {
            _sut.SetSystemCulture(LanguageCode.RuRu);

            Assert.Equal(CultureInfo.GetCultureInfo("ru-RU"), _sut.SystemCulture);
            Assert.Equal(LanguageCode.RuRu, _sut.Language);

            _sut.SetSystemCulture(LanguageCode.Default);
        }

        [Fact]
        public void SettingSystemCultureUpdatesLanguageAndCulture()
        {
            var culture = CultureInfo.GetCultureInfo("ru-RU");

            _sut.SetSystemCulture(culture);

            Assert.Equal(culture, _sut.SystemCulture);
            Assert.Equal(LanguageCode.RuRu, _sut.Language);

            _sut.SetSystemCulture(LanguageCode.Default);
        }

        [Fact]
        public void ReturnsLocalizedValueIfKeyExists()
        {
            var localizedString = _sut.GetLocalizedString("AddBotLabel");
            
            Assert.Equal("Add Bot", localizedString);
        }

        [Fact]
        public void ReturnsKeyIfKeyDoesNotExist()
        {
            const string keyThatDoesNotExist = "KeyThatDoesNotExist";
            var localizedString = _sut.GetLocalizedString(keyThatDoesNotExist);
            
            Assert.Equal(keyThatDoesNotExist, localizedString);
        }

        [Fact]
        public void ReturnsDefaultValueIfKeyIsNullOrEmpty()
        {
            var localizedString = _sut.GetLocalizedString(null);
            
            Assert.NotNull(localizedString);
            Assert.NotEmpty(localizedString);
        }

        //TODO when we add more localizations check to return default language value if value for current one does not exist
    }
}
