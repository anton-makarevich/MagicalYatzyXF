using System.Threading.Tasks;
using NSubstitute;
using Sanet.MagicalYatzy.Common.Services;
using Sanet.MagicalYatzy.Dto.Requests;
using Sanet.MagicalYatzy.Dto.Responses;
using Sanet.MagicalYatzy.Services.Api;
using Xunit;

namespace MagicalYatzyTests.Services.Api
{
    public class AzureApiClientTests
    {
        private readonly IWebService _webServiceMock = Substitute.For<IWebService>();
        private AzureApiClient _sut;

        public AzureApiClientTests()
        {
            _sut = new AzureApiClient(_webServiceMock);
        }

        [Fact]
        public async Task LoginAsyncMakesPostCallToWebService()
        {
            await _sut.LoginUserAsync("SomeName", "SomePassword");

            await _webServiceMock.Received().PostAsync<LoginResponse>(Arg.Any<LoginRequest>(), Arg.Any<string>());
        }
    }
}