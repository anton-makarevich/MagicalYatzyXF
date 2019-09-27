using System.Threading.Tasks;
using NSubstitute;
using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Requests;
using Sanet.MagicalYatzy.Dto.Responses;
using Sanet.MagicalYatzy.Dto.Services;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services.Api;
using Xunit;

namespace MagicalYatzyTests.Services.Api
{
    public class AzureApiClientTests
    {
        private readonly IWebService _webServiceMock = Substitute.For<IWebService>();
        private readonly AzureApiClient _sut;

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

        [Fact]
        public async Task LoginAsyncReturnsPlayerReturnedByWebService()
        {
            const string playerName = "SomeName";
            var responseSub = new LoginResponse() { Player = new LoginModel(){ PlayerName = playerName}};
            _webServiceMock.PostAsync<LoginResponse>(null, "")
                .ReturnsForAnyArgs(Task.FromResult<LoginResponse>(responseSub));
            
            var player = await _sut.LoginUserAsync(playerName, "SomePassword");
            Assert.Equal(playerName, player.Name);
        }

        [Fact]
        public async Task SaveScoreAsyncCallsWebServiceWithCorrespondingRequest()
        {
            const string playerName = "SomeName";
            const int score = 123;
            const Rules rule = Rules.krStandard;

            await _sut.SaveScoreAsync(playerName, score, rule);
            
            await _webServiceMock.Received().PostAsync<SaveScoreResponse>(Arg.Any<SaveScoreRequest>(), Arg.Any<string>());
        }
    }
}