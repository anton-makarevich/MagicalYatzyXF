using Newtonsoft.Json;
using Plugin.Connectivity;
using Sanet.MagicalYatzy.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Sanet.MagicalYatzy.XF.Services
{
    public class WebService : IWebService
    {
        private const string JsonContentType = "application/json";
        private readonly HttpClient _httpClient;

        public WebService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(Constants.ApiEndpoint),
                DefaultRequestHeaders =
                {
                    Accept =
                    {
                        new MediaTypeWithQualityHeaderValue(JsonContentType)
                    }
                }
            };

        }

        public async Task<T> GetAsync<T>(string url)
        {
            try
            {
                var response = await SendRequest(HttpMethod.Get, url);
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(),
                        new JsonSerializerSettings() { NullValueHandling = 0 });
                }

                return default(T);
            }
            catch
            {
                return default(T);
            }
        }

        public async Task<T> PostAsync<T>(string url)
        {
            try
            {
                var response = await SendRequest(HttpMethod.Post, url);
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(),
                        new JsonSerializerSettings() { NullValueHandling = 0 });
                }

                return default(T);
            }
            catch
            {
                return default(T);
            }
        }

        private async Task<HttpResponseMessage> SendRequest(HttpMethod method, string url, object content = null)
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    throw new Exception($"Request: {url}");
                }

                var request = new HttpRequestMessage(method, url);

                if (content != null)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, JsonContentType);
                }

                return await _httpClient.SendAsync(request);
            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }
        }
    }
}
