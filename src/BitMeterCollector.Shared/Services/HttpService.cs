using BitMeterCollector.Shared.Configuration;
using Microsoft.Extensions.Logging;

namespace BitMeterCollector.Shared.Services;

public interface IHttpService
{
  Task<string> GetUrl(string url);
}

public class HttpService : IHttpService
{
  private readonly ILogger<HttpService> _logger;
  private readonly BitMeterCollectorConfig _config;
  private readonly HttpClient _httpClient;

  public HttpService(
    ILogger<HttpService> logger,
    BitMeterCollectorConfig config)
  {
    _logger = logger;
    _config = config;

    _httpClient = new HttpClient
    {
      Timeout = TimeSpan.FromMilliseconds(_config.HttpServiceTimeoutMs)
    };
  }

  public async Task<string> GetUrl(string url)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, url);
    var response = await _httpClient.SendAsync(request);
    var responseBody = await response.Content.ReadAsStringAsync();

    return responseBody;
  }
}
