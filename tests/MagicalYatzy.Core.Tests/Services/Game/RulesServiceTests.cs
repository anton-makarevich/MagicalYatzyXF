using System.Linq;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Utils;
using Xunit;

namespace MagicalYatzyTests.Services.Game
{
    public class RulesServiceTests
    {
        private readonly RulesService _sut = new RulesService(); 
        
        [Fact]
        public void GetAllRulesReturnsAllAvailableRules()
        {
            var expectedAllRules = EnumUtils.GetValues<Rules>();
            
            var rules = _sut.GetAllRules().ToList();
            
            Assert.Equal(expectedAllRules.Length, rules.Count);
            foreach (var rule in expectedAllRules)
            {
                Assert.Contains(rule, rules);
            }
        }
    }
}