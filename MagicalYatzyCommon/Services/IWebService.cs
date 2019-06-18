using System.Threading.Tasks;

namespace Sanet.MagicalYatzy.Services.Api
{
    public interface IWebService
    {
        Task<T> GetAsync<T>(string url);
        Task<T> PostAsync<T>(string url);
    }
}