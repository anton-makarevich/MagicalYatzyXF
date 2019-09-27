using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sanet.MagicalYatzy.Dto.Requests;
using Sanet.MagicalYatzy.Dto.Responses;
using Sanet.MagicalYatzy.Dto.Services;
using Sanet.MagicalYatzy.Web.Functions.ScoreSaver.Services;

namespace Sanet.MagicalYatzy.Web.Functions.ScoreSaver
{
    public class ScoreSaverFunction
    {
        private ILeaderBoardService _leaderBoardService;

        public void SetService(ILeaderBoardService leaderBoardService)
        {
            _leaderBoardService = leaderBoardService;
        }
        [FunctionName("ScoreSaverFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "scores")] HttpRequest request,
            ILogger log)
        {
            if (_leaderBoardService == null)
                SetService(new AzureLeaderBoardService(log));

            var responseObject = new SaveScoreResponse();
            var requestData = await new StreamReader(request.Body).ReadToEndAsync();
            var requestObject = JsonConvert.DeserializeObject<SaveScoreRequest>(requestData);
            
            if (requestObject?.Score == null)
            {
                responseObject.ErrorCode = (int)HttpStatusCode.BadRequest;
                responseObject.Message = "Invalid request data";
            }
            else
            {
                if (string.IsNullOrEmpty(requestObject.Score.SeasonId))
                    requestObject.Score.SeasonId = DateTime.UtcNow.Year.ToString();
                var id = await _leaderBoardService.SaveScoreAsync(requestObject.Score);
                if (string.IsNullOrEmpty(id))
                {
                    responseObject.ErrorCode = (int)HttpStatusCode.InternalServerError;
                    responseObject.Message = "Unable to save score";
                }
                else
                {
                    requestObject.Score.ScoreId = id;
                    responseObject.ErrorCode = (int)HttpStatusCode.OK;
                    responseObject.Score = requestObject.Score;
                }
            }

            return new JsonResult(responseObject);
        }
    }
}
