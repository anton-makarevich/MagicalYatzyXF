using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Responses.Base;

namespace Sanet.MagicalYatzy.Dto.Responses
{
    public class LoginResponse:ResponseBase
    {
        public LoginModel? Player { get; set; }
    }
}