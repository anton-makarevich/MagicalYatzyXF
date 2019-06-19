using System.Threading.Tasks;
using NSubstitute;
using Sanet.MagicalYatzy.Common.Services;
using Sanet.MagicalYatzy.Services.Api;
using Xunit;

namespace MagicalYatzyTests.Services.Api
{
    public class ApiClientTests
    {
        private readonly ApiClient _sut;
        private readonly IWebService _webService = Substitute.For<IWebService>();

        public ApiClientTests()
        {
            _sut = new ApiClient(_webService);
        }

        [Fact]
        public async Task LoginUserAsyncCallsWebServicePost()
        {
            await _sut.LoginUserAsync("", "");

            await _webService.Received().PostAsync<bool>(Arg.Any<string>());
        }
    }
}