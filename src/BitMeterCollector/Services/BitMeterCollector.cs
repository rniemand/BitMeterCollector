using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitMeterCollector.Configuration;
using BitMeterCollector.Metrics;
using BitMeterCollector.Models;
using Microsoft.Extensions.Logging;

namespace BitMeterCollector.Services
{
  public interface IBitMeterCollector
  {
    Task Tick();
  }

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
      foreach (var server in GetServers())
      {
        // Get the raw data line from BitMeter
        var response = await GetStatsResponse(server);
        if (response == null) continue;

        // Generate and send the metric
        var metric = _metricFactory.FromStatsResponse(response);
        _metricService.EnqueueMetric(metric);
      }
    }

    private IEnumerable<BitMeterEndPointConfig> GetServers()
    {
      // TODO: [TESTS] (BitMeterCollector.GetServers) Add tests

      return _config.Servers.Where(s => s.Enabled).ToList();
    }

    private async Task<StatsResponse> GetStatsResponse(BitMeterEndPointConfig endpoint)
    {
      // TODO: [TESTS] (BitMeterCollector.GetStatsResponse) Add tests
      // TODO: [LOGGING] (BitMeterCollector.GetStatsResponse) Add logging
      // TODO: [COMPLETE] (BitMeterCollector.GetStatsResponse) Disable a server if it misses "x" polls

      var url = endpoint.BuildUrl("getStats");

      try
      {
        var body = await _httpService.GetUrl(url);

        if (_responseParser.TryParseStatsResponse(endpoint, body, out var parsed))
        {
          return parsed;
        }
      }
      catch (Exception ex)
      {
        // TODO: [COMPLETE] (BitMeterCollector.GetStatsResponse) Add HumanStackTrace()

        _logger.LogError(ex,
          "Unable to GET {url}. {exType}: {exMessage}",
          url,
          ex.GetType().Name,
          ex.Message
        );
      }

      return null;
    }
  }
}