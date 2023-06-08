using BitMeterCollector.Shared.Configuration;

namespace BitMeterCollector.Shared.Services;

public interface IHttpService
{
  Task<string> GetUrl(string url);
}

public class HttpService : IHttpService
{
  private readonly HttpClient _httpClient;

  public HttpService(BitMeterConfig config)
  {
    var httpClient = new HttpClient();
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
