using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Sanet.MagicalYatzy.Web.Functions.Login
{
    public class LoginFunction
    {
        [FunctionName("LoginFunction")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "players")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            return name != null
                ? (ActionResult)new OkObjectResult($"Hi, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}