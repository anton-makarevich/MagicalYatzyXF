using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sanet.MagicalYatzy.Dto.Requests;
using Sanet.MagicalYatzy.Dto.Responses;
using Sanet.MagicalYatzy.Dto.Services;

namespace Sanet.MagicalYatzy.Web.Functions.Login
{
    public class LoginFunction
    {
        private readonly ILoginService _loginService;

        public LoginFunction(ILoginService loginService)
        {
            _loginService = loginService;
        }
        
        [FunctionName("LoginFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "players")]
            HttpRequest request, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var responseObject = new LoginResponse();
            var requestData = await new StreamReader(request.Body).ReadToEndAsync();
            var requestObject = JsonConvert.DeserializeObject<LoginRequest>(requestData);

            if (requestObject?.Player == null)
            {
                responseObject.ErrorCode = (int)HttpStatusCode.BadRequest;
                responseObject.Message = "Invalid request data";
            }
            else
            {
                try
                {
                    await _loginService.LoginAsync(requestObject.Player);
                    responseObject.Player = requestObject.Player;
                }
                catch (Exception e)
                {
                    log.LogError("Trying to make wcf call",e);
                    responseObject.ErrorCode = (int)HttpStatusCode.BadRequest;
                    responseObject.Message = "Invalid request data";
                }
                
            }
            return new JsonResult(responseObject);
        }
    }
}