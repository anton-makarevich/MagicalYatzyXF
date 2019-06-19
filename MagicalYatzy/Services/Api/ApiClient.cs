using System.Net;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Common.Services;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.Api
{
    public class ApiClient : IApiClient
    {
        private readonly IWebService _webService;

        public ApiClient(IWebService webService)
        {
            _webService = webService;
        }

        public async Task<IPlayer> LoginUserAsync(string username, string password)
        {
            var url = $"YatzyUsers?username={WebUtility.UrlEncode(username.Encrypt(33))}&pass={WebUtility.UrlEncode(password.Encrypt(33))}&token={WebUtility.UrlEncode("33".Encrypt(33))}";
            await _webService.PostAsync<bool>(url);
            return null;
        }
    }
}
