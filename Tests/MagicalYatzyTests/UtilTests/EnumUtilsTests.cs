using Xunit;
using Sanet.MagicalYatzy.Utils;

namespace MagicalYatzyTests.UtilTests
{
    public class EnumUtilsTests
    {
        [Fact]
        public void LoopsThroughEnumeration()
        {
            var enumValues = EnumUtils.GetValues<TestEnum>();
            Assert.Equal(3, enumValues.Length);

            for (int i = 0; i < 3; i++)
                Assert.Equal($"Case{i+1}", enumValues[i].ToString());
        }

        private enum TestEnum
        {
            Case1,
            Case2,
            Case3
        }
    }
}
