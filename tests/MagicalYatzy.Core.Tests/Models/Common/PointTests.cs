using Sanet.MagicalYatzy.Models.Common;
using Xunit;

namespace MagicalYatzyTests.Models.Common
{
    public class PointTests
    {
        [Fact]
        public void ItIsPossibleToSumPoints()
        {
            Point point1 = new Point(20, 30);
            Point point2 = new Point(30, 40);

            var resultinPoint = point1 + point2;

            Assert.Equal(20 + 30, resultinPoint.X);
            Assert.Equal(30 + 40, resultinPoint.Y);
        }
    }
}
