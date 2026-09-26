using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Dto.ApiConfigs;

namespace Sanet.MagicalYatzy.Dto.Services;

public class WebService : IWebService
{
    private const string JsonContentType = "application/json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly HttpClient _httpClient;

    public WebService(IApiConfig config)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(Path.Combine(config.BaseUrl,config.VersionSuffix)),
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
                return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), JsonOptions);
            }
#nullable disable
            return default;
        }
        catch
        {
            return default;
        }
#nullable enable
    }

    public async Task<T> PostAsync<T>(object requestModel, string url)
    {
        try
        {
            var response = await SendRequest(HttpMethod.Post, url, requestModel);
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), JsonOptions);
            }
#nullable disable
            return default;
        }
        catch
        {
            return default;
        }
#nullable enable
    }

    private async Task<HttpResponseMessage> SendRequest(HttpMethod method, string url, object? content = null)
    {
        try
        {
            var request = new HttpRequestMessage(method, url);

            if (content != null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(content, content.GetType(), JsonOptions), Encoding.UTF8, JsonContentType);
            }

            return await _httpClient.SendAsync(request);
        }
        catch
        {
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        }
    }
}
