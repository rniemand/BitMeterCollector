using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitMeterCollector.Abstractions;
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
    private readonly IDateTimeAbstraction _dateTime;

    public BitMeterCollector(
      ILogger<BitMeterCollector> logger,
      BitMeterCollectorConfig config,
      IHttpService httpService,
      IResponseParser responseParser,
      IMetricFactory metricFactory,
      IMetricService metricService,
      IDateTimeAbstraction dateTime)
    {
      _logger = logger;
      _config = config;
      _httpService = httpService;
      _responseParser = responseParser;
      _metricFactory = metricFactory;
      _metricService = metricService;
      _dateTime = dateTime;
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
      var currentTime = _dateTime.Now;

      return _config.Servers
        .Where(s => s.CanCollectStats(currentTime))
        .ToList();
    }

    private void HandleServerBackOff(BitMeterEndPointConfig endpoint)
    {
      // TODO: [TESTS] (BitMeterCollector.HandleServerBackOff) Add tests

      if (!endpoint.UnsuccessfulPoll())
        return;

      var backOffEndTime = _dateTime.Now.AddSeconds(_config.BackOffPeriodSeconds);
      endpoint.SetBackOffEndTime(backOffEndTime);

      _logger.LogInformation(
        "Unable to reach {server} - backing off for {time} seconds (will try again at {date})",
        endpoint.ServerName,
        _config.BackOffPeriodSeconds,
        backOffEndTime
      );
    }

    private async Task<StatsResponse> GetStatsResponse(BitMeterEndPointConfig endpoint)
    {
      // TODO: [TESTS] (BitMeterCollector.GetStatsResponse) Add tests
      // TODO: [LOGGING] (BitMeterCollector.GetStatsResponse) Add logging

      var url = endpoint.BuildUrl("getStats");
      var mustBackOff = false;

      try
      {
        var body = await _httpService.GetUrl(url);
        if (_responseParser.TryParseStatsResponse(endpoint, body, out var parsed))
        {
          endpoint.SuccessfulPoll();
          return parsed;
        }
      }
      catch (TaskCanceledException)
      {
        mustBackOff = true;
        _logger.LogWarning(
          "Timed out after {time} ms getting stats from {server}",
          _config.HttpServiceTimeoutMs,
          endpoint.ServerName
        );
      }
      catch (Exception ex)
      {
        // TODO: [COMPLETE] (BitMeterCollector.GetStatsResponse) Add HumanStackTrace()
        mustBackOff = true;
        _logger.LogError(
          "{type} thrown getting stats from {server}: {stack}",
          ex.GetType().Name,
          endpoint.ServerName,
          ex.Message
        );
      }
      finally
      {
        if (mustBackOff) HandleServerBackOff(endpoint);
      }

      return null;
    }
  }
}