using System.Net.Http;
using System.Threading.Tasks;
using BitMeterCollector.Services.Interfaces;

namespace BitMeterCollector.Services
{
  public class HttpService : IHttpService
  {
    private readonly HttpClient _httpClient;

    public HttpService()
    {
      _httpClient = new HttpClient();
    }

    public async Task<string> GetUrl(string url)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, url);
      var response = await _httpClient.SendAsync(request);
      var responseBody = await response.Content.ReadAsStringAsync();

      return responseBody;
    }
  }
}