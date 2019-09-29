using System.Threading.Tasks;

namespace Sanet.MagicalYatzy.Dto.Services
{
    public interface IWebService
    {
        Task<T> GetAsync<T>(string url);
        Task<T> PostAsync<T>(object requestModel, string url);
    }
}