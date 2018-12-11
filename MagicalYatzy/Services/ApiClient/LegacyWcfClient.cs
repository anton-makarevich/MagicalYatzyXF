using System.ServiceModel;
using System.Threading.Tasks;
using MagicalYatzy.LegacyWcfService;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services
{
    public class LegacyWcfClient:IApiClient
    {
        public async Task<IPlayer> LoginUserAsync(string username, string password)
        {
            var address = new System.ServiceModel.EndpointAddress("http://sanet.by/KniffelService.asmx");
            BasicHttpBinding bind = new BasicHttpBinding();
            var client = new KniffelServiceSoapClient(bind,address);
            int rolls = 0;
            int manuals = 0;
            int resets = 0;
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
            catch 
            { 
                return null; 
            }
            finally
            {
                client.CloseAsync();
            }

            return null;
        }
    }
}
