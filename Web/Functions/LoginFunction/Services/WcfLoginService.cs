using System.ServiceModel;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Dto.ConnectedServices.LegacyScoreService;
using Sanet.MagicalYatzy.Dto.Extensions;
using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Services;
using Sanet.MagicalYatzy.Extensions;

namespace Sanet.MagicalYatzy.Web.Functions.Login.Services
{
    public class WcfLoginService:ILoginService
    {
        public async Task<LoginModel> LoginAsync(LoginModel loginModel)
        {
            var address = new EndpointAddress("http://sanet.by/KniffelService.asmx");
            var bind = new BasicHttpBinding();
            var client = new KniffelServiceSoapClient(bind,address);
            const int rolls = 0;
            const int manuals = 0;
            const int resets = 0;
            var password = loginModel.Password.Encrypt(33);
            try
            {
                var result = await client.GetPlayersMagicsTaskAsync(
                    loginModel.PlayerName, password,
                    rolls,
                    manuals,
                    resets);

                if (result.Body.GetPlayersMagicsResult)
                {
                    loginModel.Password = password;
                    return loginModel;
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