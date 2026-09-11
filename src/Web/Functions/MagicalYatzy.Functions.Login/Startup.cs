using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Dto.Services;
using Sanet.MagicalYatzy.Web.Functions.Login;
using Sanet.MagicalYatzy.Web.Functions.Login.Services;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Sanet.MagicalYatzy.Web.Functions.Login
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ILoginService, WcfLoginService>();
        }
    }
}
