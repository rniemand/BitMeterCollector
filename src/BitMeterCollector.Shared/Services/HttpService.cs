using BitMeterCollector.Shared.Configuration;
using Rn.NetCore.BasicHttp;

namespace BitMeterCollector.Shared.Services;

public interface IHttpService
{
  Task<string> GetUrl(string url);
}

public class HttpService : IHttpService
{
  private readonly IHttpClient _httpClient;

  public HttpService(BitMeterConfig config, IHttpClientFactory httpClientFactory)
  {
    var httpClient = httpClientFactory.GetHttpClient();
    httpClient.Timeout = TimeSpan.FromMilliseconds(config.HttpServiceTimeoutMs);
    _httpClient = httpClient;
  }

  public async Task<string> GetUrl(string url)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, url);
    var response = await _httpClient.SendAsync(request);
    var responseBody = await response.Content.ReadAsStringAsync();

    return responseBody;
  }
}
