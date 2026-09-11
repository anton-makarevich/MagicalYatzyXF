using Sanet.MagicalYatzy.Models.Common;
using Xunit;

namespace MagicalYatzyTests.Models.Common
{
    public class RectangleTests
    {
        [Fact]
        public void ContainsReturnsFalseIfOtherRectangleIsNotWithinThisOne()
        {
            var outerRect = new Rectangle(0, 0, 30, 30);
            var innerRect  = new Rectangle(10,10,40,40);
            
            Assert.False(outerRect.Contains(innerRect));
        }
    }
}