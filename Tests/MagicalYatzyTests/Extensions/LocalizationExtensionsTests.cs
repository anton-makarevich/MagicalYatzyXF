using System.Globalization;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models;
using Xunit;

namespace MagicalYatzyTests.Extensions
{
    public class LocalizationExtensionsTests
    {
        [Fact]
        public void ReturnsDefaultLanguageCodeForUnknownCulture()
        {
            var sut = new CultureInfo("nl");

            var languageCode = sut.ToLanguageCode();
            
            Assert.Equal(LanguageCode.Default, languageCode);
        }
    }
}