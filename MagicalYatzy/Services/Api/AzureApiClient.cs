using System.Threading.Tasks;
using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Requests;
using Sanet.MagicalYatzy.Dto.Responses;
using Sanet.MagicalYatzy.Dto.Services;
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
        public async Task<IPlayer> LoginUserAsync(string username, string password)
        {
            var loginModel = new LoginModel() {PlayerName = username, Password = password};
            var response = await _webService.PostAsync<LoginResponse>(new LoginRequest()
                {
                    Player = loginModel
                },
                "players");
            if (response?.Player != null && response.ErrorCode == 0)
            {
                return new Player(){ Name = response.Player.PlayerName};
            }

            return null;
        }
    }
}