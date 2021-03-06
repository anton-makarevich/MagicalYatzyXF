using System;
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
using Xunit;
using Sanet.MagicalYatzy.Web.Functions.ScoreSaver;

namespace ScoreSaverTests
{
    public class ScoreSaverFunctionTests
    {
        private readonly ScoreSaverFunction _sut;
        private readonly ILeaderBoardService _leaderBoardServiceMock;

        public ScoreSaverFunctionTests()
        {
            _leaderBoardServiceMock = Substitute.For<ILeaderBoardService>();
            _sut = new ScoreSaverFunction();
            _sut.SetService(_leaderBoardServiceMock);
        }

        [Fact]
        public async Task RunningTheFunctionCallsSaveScoreOnLeaderBoard()
        {
            var score = new PlayerScore();

            await _sut.Run(Utils.CreateMockRequest(
                    new SaveScoreRequest(){Score = score}),
                Substitute.For<ILogger>());

            await _leaderBoardServiceMock.Received().SaveScoreAsync(Arg.Any<PlayerScore>());
        }

        [Fact]
        public async Task RunningFunctionWithoutProperRequestReturnsBadRequestErrorCode()
        {
            var actionResult = await _sut.Run(Utils.CreateMockRequest(
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
            
            var actionResult = await _sut.Run(Utils.CreateMockRequest(
                    new SaveScoreRequest(){Score = score}),
                Substitute.For<ILogger>())as JsonResult;
            
            Assert.NotNull(actionResult);
            var response = actionResult.Value as SaveScoreResponse;
            
            Assert.NotNull(response?.Score);
            Assert.Equal(200,response.ErrorCode);
            Assert.Equal(scoreId,response.Score.ScoreId);
        }
        
        [Fact]
        public async Task RunFunctionReturnsInternalServerErrorIfServiceDoesNotProvideScoreId()
        {
            var score = new PlayerScore();
            _leaderBoardServiceMock.SaveScoreAsync(score).ReturnsForAnyArgs(Task.FromResult<string>(null));
            
            var actionResult = await _sut.Run(Utils.CreateMockRequest(
                    new SaveScoreRequest(){Score = score}),
                Substitute.For<ILogger>())as JsonResult;
            
            Assert.NotNull(actionResult);
            var response = actionResult.Value as SaveScoreResponse;
            
            Assert.Null(response?.Score);
            Assert.Equal(500,response?.ErrorCode);
        }
        
        [Fact]
        public async Task IfScoreDoesNotContainsSeasonItSavedAsCurrentYearAsSeason()
        {
            var season = DateTime.UtcNow.Year.ToString();
            var score = new PlayerScore();
            _leaderBoardServiceMock.SaveScoreAsync(score).ReturnsForAnyArgs(Task.FromResult("123"));
            
            var actionResult = await _sut.Run(Utils.CreateMockRequest(
                    new SaveScoreRequest(){Score = score}),
                Substitute.For<ILogger>())as JsonResult;
            
            Assert.NotNull(actionResult);
            var response = actionResult.Value as SaveScoreResponse;
            
            Assert.NotNull(response?.Score);
            Assert.Equal(200,response.ErrorCode);
            Assert.Equal(season,response.Score.SeasonId);
        }
    }
}