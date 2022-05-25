using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Extensions;
using BitMeterCollector.Shared.Factories;
using BitMeterCollector.Shared.Models;
using Microsoft.Extensions.Logging;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Metrics;

namespace BitMeterCollector.Shared.Services;

public interface IBitMeterCollector
{
  Task TickAsync(CancellationToken stoppingToken);
}

public class BitMeterCollector : IBitMeterCollector
{
  private readonly ILogger<BitMeterCollector> _logger;
  private readonly BitMeterConfig _config;
  private readonly IHttpService _httpService;
  private readonly IResponseService _responseService;
  private readonly IMetricFactory _metricFactory;
  private readonly IMetricService _metricService;
  private readonly IDateTimeAbstraction _dateTime;

  public BitMeterCollector(
    ILogger<BitMeterCollector> logger,
    BitMeterConfig config,
    IHttpService httpService,
    IResponseService responseService,
    IMetricFactory metricFactory,
    IMetricService metricService,
    IDateTimeAbstraction dateTime)
  {
    _logger = logger;
    _config = config;
    _httpService = httpService;
    _responseService = responseService;
    _metricFactory = metricFactory;
    _metricService = metricService;
    _dateTime = dateTime;
  }

  public async Task TickAsync(CancellationToken stoppingToken)
  {
    foreach (var server in GetServers())
    {
      // Get the raw data line from BitMeter
      var response = await GetStatsResponse(server);
      if (response == null)
        continue;

      // Generate and send the metric
      var metric = _metricFactory.FromStatsResponse(response);
      await _metricService.SubmitMetricAsync(metric);
    }

    await Task.Delay(_config.CollectionIntervalSec * 1000, stoppingToken);
  }

  private IEnumerable<BitMeterEndPointConfig> GetServers()
  {
    var currentTime = _dateTime.Now;

    return _config.Servers
      .Where(s => s.CanCollectStats(currentTime))
      .ToList();
  }

  private void HandleServerBackOff(BitMeterEndPointConfig endpoint)
  {
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
    var url = endpoint.BuildUrl("getStats");
    var mustBackOff = false;

    try
    {
      var body = await _httpService.GetUrl(url);
      if (_responseService.TryParseStatsResponse(endpoint, body, out var parsed))
      {
        endpoint.SuccessfulPoll();
        return parsed;
      }
    }
    catch (TaskCanceledException)
    {
      mustBackOff = true;
      _logger.LogWarning("Timed out after {time} ms getting stats from {server}",
        _config.HttpServiceTimeoutMs,
        endpoint.ServerName
      );
    }
    catch (Exception ex)
    {
      mustBackOff = true;
      _logger.LogError(ex, ex.AsGenericError());
    }
    finally
    {
      if (mustBackOff) HandleServerBackOff(endpoint);
    }

    return null;
  }
}
