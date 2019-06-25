using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Requests;
using Sanet.MagicalYatzy.Dto.Responses;
using Sanet.MagicalYatzy.Dto.Services;
using Xunit;
using Sanet.MagicalYatzy.Web.Functions.ScoreSaver;

namespace ScoreSaverTests
{
    public class ScoreSaverFunctionTests
    {
        private ScoreSaverFunction _sut;
        private ILeaderBoardService _leaderBoardServiceMock;

        public ScoreSaverFunctionTests()
        {
            _leaderBoardServiceMock = Substitute.For<ILeaderBoardService>();
            _sut = new ScoreSaverFunction(_leaderBoardServiceMock);
        }

        [Fact]
        public async Task RunningTheFunctionCallsSaveScoreOnLeaderBoard()
        {
            var score = new PlayerScore();

            await _sut.Run(CreateMockRequest(
                    new SaveScoreRequest(){Score = score}),
                Substitute.For<ILogger>());

            await _leaderBoardServiceMock.Received().SaveScoreAsync(Arg.Any<PlayerScore>());
        }

        [Fact]
        public async Task RunningFunctionWithoutProperRequestReturnsBadRequestErrorCode()
        {
            var actionResult = await _sut.Run(CreateMockRequest(
                    new SaveScoreRequest(){Score = null}),
                Substitute.For<ILogger>()) as JsonResult;
            
            Assert.NotNull(actionResult);
            var response = actionResult.Value as SaveScoreResponse;
            
            Assert.NotNull(response);
            const int badRequestStatus = (int) HttpStatusCode.BadRequest;
            Assert.Equal(badRequestStatus, response.ErrorCode);
        }

        [Fact]
        public async Task RunFunctionReturnsSavedScoreWithIdReturnedByService()
        {
            var scoreId = "123";
            var score = new PlayerScore();
            _leaderBoardServiceMock.SaveScoreAsync(score).ReturnsForAnyArgs(Task.FromResult(scoreId));
            
            var actionResult = await _sut.Run(CreateMockRequest(
                    new SaveScoreRequest(){Score = score}),
                Substitute.For<ILogger>())as JsonResult;
            
            Assert.NotNull(actionResult);
            var response = actionResult.Value as SaveScoreResponse;
            
            Assert.NotNull(response?.Score);
            Assert.Equal(scoreId,response.Score.ScoreId);
        }
        
        private static HttpRequest CreateMockRequest(object body)
        {            
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
 
            var json = JsonConvert.SerializeObject(body);
 
            sw.Write(json);
            sw.Flush();
 
            ms.Position = 0;

            var mockRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = ms
            };
 
            return mockRequest;
        }
    }
}