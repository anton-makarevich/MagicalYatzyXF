using System.Threading.Tasks;
using Sanet.MagicalYatzy.Common.Services;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.Api
{
    public class AzureApiClient:IApiClient
    {
        private readonly IWebService _webService;

        public AzureApiClient(IWebService webService)
        {
            _webService = webService;
        }
        public Task<IPlayer> LoginUserAsync(string username, string password)
        {
            throw new System.NotImplementedException();
        }
    }
}