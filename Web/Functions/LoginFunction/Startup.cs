using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Web.Functions.Login;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Sanet.MagicalYatzy.Web.Functions.Login
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            
        }
    }
}
