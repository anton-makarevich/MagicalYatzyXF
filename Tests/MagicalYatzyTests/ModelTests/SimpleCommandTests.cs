using Sanet.MagicalYatzy.Models;
using Xunit;

namespace MagicalYatzyTests.ModelTests
{
    public class SimpleCommandTests
    {
        [Fact]
        public void CanAlwaysBeExecuted()
        {
            var sut = new SimpleCommand(() => { });
            
            Assert.True(sut.CanExecute(null));
        }
    }
}