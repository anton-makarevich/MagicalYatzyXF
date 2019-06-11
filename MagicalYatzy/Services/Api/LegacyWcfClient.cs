using System.ServiceModel;
using System.Threading.Tasks;
using MagicalYatzy.LegacyWcfService;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.Api
{
    public class LegacyWcfClient:IApiClient
    {
        public LegacyWcfClient()
        {
            
        }
        public async Task<IPlayer> LoginUserAsync(string username, string password)
        {
            var address = new EndpointAddress("http://sanet.by/KniffelService.asmx");
            var bind = new BasicHttpBinding();
            var client = new KniffelServiceSoapClient(bind,address);
            const int rolls = 0;
            const int manuals = 0;
            const int resets = 0;
            password = password.Encrypt(33);
            try
            {
                var result = await client.GetPlayersMagicsTaskAsync(username, password, rolls, manuals, resets);

                if (result.Body.GetPlayersMagicsResult)
                {
                    return new Player()
                    {
                        Name = username,
                        Password = password
                    };
                }
            }
            finally
            {
                client.CloseAsync();
            }

            return null;
        }
    }
}
