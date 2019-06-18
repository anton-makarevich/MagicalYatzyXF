using Sanet.MagicalYatzy;
using Sanet.MagicalYatzy.Common;
using Xunit;

namespace MagicalYatzyTests.Utils
{
    public class ConstantsTests
    {
        [Fact]
        public void ApiEndpointIsCorrect()
        {
            Assert.Equal("http://sanet.by/api/", Constants.ApiEndpoint);
        }
    }
}
