using System.Linq;
using System.Threading.Tasks;
using BitMeterCollector.Configuration;
using BitMeterCollector.Metrics.Interfaces;
using BitMeterCollector.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BitMeterCollector.Services
{
  public class BitMeterCollector : IBitMeterCollector
  {
    private readonly ILogger<BitMeterCollector> _logger;
    private readonly BitMeterCollectorConfig _config;
    private readonly IHttpService _httpService;
    private readonly IResponseParser _responseParser;
    private readonly IMetricFactory _metricFactory;
    private readonly IMetricService _metricService;

    public BitMeterCollector(
      ILogger<BitMeterCollector> logger,
      BitMeterCollectorConfig config,
      IHttpService httpService,
      IResponseParser responseParser,
      IMetricFactory metricFactory,
      IMetricService metricService)
    {
      _logger = logger;
      _config = config;
      _httpService = httpService;
      _responseParser = responseParser;
      _metricFactory = metricFactory;
      _metricService = metricService;
    }

    public async Task Tick()
    {
      var servers = _config.Servers.Where(s => s.Enabled).ToList();

      foreach (var server in servers)
      {
        var url = server.BuildUrl("getStats");
        var body = await _httpService.GetUrl(url);

        if (_responseParser.TryParseStatsResponse(server, body, out var parsed))
        {
          _metricService.EnqueueMetric(_metricFactory.FromStatsResponse(parsed));
        }
      }


    }
  }
}