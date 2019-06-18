using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Services;
using Xunit;
using Sanet.MagicalYatzy.Web.Functions;

namespace ScoreSaverTests
{
    public class ScoreSaverFunctionTests
    {
        [Fact]
        public async Task RunningTheFunctionCallsSaveScoreOnLeaderBoard()
        {
            var score = new PlayerScore();
            var leaderBoardServiceMock = Substitute.For<ILeaderBoardService>();
            var sut = new ScoreSaverFunction(leaderBoardServiceMock);

            await sut.Run(
                new DefaultHttpRequest(new DefaultHttpContext()),
                Substitute.For<ILogger>());

            await leaderBoardServiceMock.Received().SaveScoreAsync(Arg.Any<PlayerScore>());
        }
    }
}