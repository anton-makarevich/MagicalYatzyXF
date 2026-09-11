using System.Net;
using System.Threading.Tasks;
using FunctionTestUtils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Requests;
using Sanet.MagicalYatzy.Dto.Responses;
using Sanet.MagicalYatzy.Dto.Services;
using Sanet.MagicalYatzy.Web.Functions.Login;
using Xunit;

namespace LoginTests
{
    public class LoginFunctionTests
    {
        private readonly LoginFunction _sut;
        private readonly ILoginService _loginServiceMock;

        public LoginFunctionTests()
        {
            _loginServiceMock = Substitute.For<ILoginService>();
            _sut = new LoginFunction(_loginServiceMock);
        }

        [Fact]
        public async Task RunningTheFunctionCallsLoginMethodOnService()
        {
            var player = new LoginModel() { PlayerName = "player", Password = "123456"};

            await _sut.Run(Utils.CreateMockRequest(
                    new LoginRequest(){Player = player}),
                Substitute.For<ILogger>());

            await _loginServiceMock.Received().LoginAsync(Arg.Any<LoginModel>());
        }

        [Fact]
        public async Task RunningFunctionWithoutProperRequestReturnsBadRequestErrorCode()
        {
            var actionResult = await _sut.Run(Utils.CreateMockRequest(
                    new SaveScoreRequest(){Score = null}),
                Substitute.For<ILogger>()) as JsonResult;
            
            Assert.NotNull(actionResult);
            var response = actionResult.Value as LoginResponse;
            
            Assert.NotNull(response);
            const int badRequestStatus = (int) HttpStatusCode.BadRequest;
            Assert.Equal(badRequestStatus, response.ErrorCode);
        }

        [Fact]
        public async Task RunFunctionReturnsLoginModel()
        {
            var loginModel = new LoginModel();
            _loginServiceMock.LoginAsync(loginModel).ReturnsForAnyArgs(Task.FromResult(loginModel));

            var actionResult = await _sut.Run(Utils.CreateMockRequest(
                    new LoginRequest() {Player = loginModel}),
                Substitute.For<ILogger>()) as JsonResult;

            Assert.NotNull(actionResult);
            var response = actionResult.Value as LoginResponse;

            Assert.NotNull(response?.Player);
        }
    }
}