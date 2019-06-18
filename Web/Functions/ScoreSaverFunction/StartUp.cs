using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Dto.Services;
using Sanet.MagicalYatzy.Web.Functions;
using Sanet.MagicalYatzy.Web.Functions.Services;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Sanet.MagicalYatzy.Web.Functions
{
    public class Startup: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ILeaderBoardService, AzureLeaderBoardService>();
        }
    }
}