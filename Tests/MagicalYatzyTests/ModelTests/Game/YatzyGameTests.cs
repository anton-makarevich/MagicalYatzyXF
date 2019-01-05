using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Utils;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Game
{
    public class YatzyGameTests
    {
        private readonly YatzyGame _sut;

        public YatzyGameTests()
        {
            _sut = new YatzyGame();
        }

        [Fact]
        public void DefaultGameRuleIsExtended()
        {
            Assert.Equal(Rules.krExtended,_sut.Rules.CurrentRule);
        }

        [Fact]
        public void GameHasRulesPassedToConstructor()
        {
            var allRules = EnumUtils.GetValues<Rules>();

            foreach (var rule in allRules)
            {
                var sut = new YatzyGame(rule);
                Assert.Equal(rule,sut.Rules.CurrentRule);
            }
        }

        [Fact]
        public void PlayersAreInitialisedWhenGameIsConstructed()
        {
            Assert.NotNull(_sut.Players);
            
            var sut = new YatzyGame(Rules.krBaby);
            Assert.NotNull(sut.Players);
        }

        [Fact]
        public void EveryGameHasAnId()
        {
            Assert.NotNull(_sut.GameId);
            Assert.NotEmpty(_sut.GameId);
            
            var sut = new YatzyGame(Rules.krBaby);
            Assert.NotNull(_sut.GameId);
            Assert.NotEmpty(_sut.GameId);
        }

        [Fact]
        public void FalseIsDefaultValueForIsPlaying()
        {
            Assert.False(_sut.IsPlaying);
        }

        [Fact]
        public void OneIsDefaultRollValue()
        {
            Assert.Equal(1,_sut.Roll);
        }

        [Fact]
        public void DefaultLastDiceResultHasEmptyResults()
        {
            Assert.NotNull(_sut.LastDiceResult?.DiceResults);
            Assert.Empty(_sut.LastDiceResult?.DiceResults);
        }

        [Fact]
        public void FalseIsDefaultReRollModeValue()
        {
            Assert.False(_sut.ReRollMode);
        }

        [Fact]
        public void ByDefaultThereAreNoPlayers()
        {
            Assert.Equal(0,_sut.NumberOfPlayers);
        }

        [Fact]
        public void ThereAreNoFixedDiceInitially()
        {
            Assert.Equal(0, _sut.NumberOfFixedDice);
        }
    }
}