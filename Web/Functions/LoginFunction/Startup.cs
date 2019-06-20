using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Web.Functions.Login;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Sanet.MagicalYatzy.Web.Functions.Login
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IGetHello, GetHelloClass>();
        }
    }

    public interface IGetHello
    {
        string GetHello(string name);
    }

    public class GetHelloClass : IGetHello
    {
        public string GetHello(string name)
        {
            return $"Hello {name}!";
        }
    }
}
