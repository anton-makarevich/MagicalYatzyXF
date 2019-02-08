using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Xunit;

namespace MagicalYatzyTests.ViewModelTests.ObservableWrappers
{
    public class RuleViewModelTests
    {
        private readonly RuleViewModel _sut;
        private readonly IRulesService _rulesService = Substitute.For<IRulesService>();
        private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
        private const Rules TestRule = Rules.krSimple;

        public RuleViewModelTests()
        {
            _sut = new RuleViewModel(TestRule, _rulesService, _localizationService);
        }

        [Fact]
        public void HasRuleProvidedThroughConstructor()
        {
            Assert.Equal(TestRule, _sut.Rule);
        }

        [Fact]
        public void HasCorrectLocalizedNameReturnedByLocalizationService()
        {
            const string expectedName = "Localized Name";

            _localizationService.GetLocalizedString(TestRule.ToString()).Returns(expectedName);
            var name = _sut.LocalizedName;
            
            Assert.Equal(expectedName.ToUpper(), name);
        }
        
        [Fact]
        public void HasCorrectLocalizedDescriptionReturnedByLocalizationService()
        {
            const string expectedDescription = "Localized Description";
            
            _localizationService.GetLocalizedString(TestRule.ToString()+ "Short").Returns(expectedDescription);
            var shortDescription = _sut.LocalizedShortDescription;
            
            Assert.Equal(expectedDescription, shortDescription);
        }
    }
}