﻿using Xunit;
using Sanet.MagicalYatzy;

namespace MagicalYatzyTests.UtilTests
{
    public class ConstantsTests
    {
        [Fact]
        public void ApiEndpointShouldBeCorrect()
        {
            Assert.Equal("http://sanet.by/api/", Constants.ApiEndpoint);
        }
    }
}
