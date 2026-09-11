using System.Threading.Tasks;
using Sanet.MagicalYatzy.Dto.Models;

namespace Sanet.MagicalYatzy.Dto.Services
{
    public interface ILoginService
    {
        Task<LoginModel> LoginAsync(LoginModel loginModel);
    }
}