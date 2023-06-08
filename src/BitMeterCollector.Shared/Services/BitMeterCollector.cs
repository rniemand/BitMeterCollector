using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Extensions;
using BitMeterCollector.Shared.Metrics;
using BitMeterCollector.Shared.Models;
using RnCore.Abstractions;
using RnCore.Logging;
using RnCore.Metrics;

namespace BitMeterCollector.Shared.Services;

public interface IBitMeterCollector
{
  Task TickAsync(CancellationToken stoppingToken);
}

public class BitMeterCollector : IBitMeterCollector
{
  private readonly ILoggerAdapter<BitMeterCollector> _logger;
  private readonly BitMeterConfig _config;
  private readonly IHttpService _httpService;
  private readonly IResponseService _responseService;
  private readonly IMetricsService _metricService;
  private readonly IDateTimeAbstraction _dateTime;

  public BitMeterCollector(
    ILoggerAdapter<BitMeterCollector> logger,
    BitMeterConfig config,
    IHttpService httpService,
    IResponseService responseService,
    IMetricsService metricService,
    IDateTimeAbstraction dateTime)
  {
    _logger = logger;
    _config = config;
    _httpService = httpService;
    _responseService = responseService;
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
      await _metricService.SubmitAsync(new BitmeterMetricBuilder(response));
    }

    await Task.Delay(_config.CollectionIntervalSec * 1000, stoppingToken);
  }

  private IEnumerable<BitMeterEndPointConfig> GetServers()
  {
    var currentTime = _dateTime.Now;

    return _config.Servers
      .Where(s => s.Enabled && s.CanCollectStats(currentTime))
      .ToList();
  }

  private void HandleBackOff(BitMeterEndPointConfig endpoint)
  {
    endpoint.MissedPolls += 1;
    if (endpoint.MissedPolls < endpoint.MaxMissedPolls)
      return;

    var backOffEndTime = _dateTime.Now.AddSeconds(_config.BackOffPeriodSeconds);
    endpoint.BackOffEndTime = backOffEndTime;

    _logger.LogInformation(
      "Unable to reach {server} - backing off for {time} seconds (will try again at {date})",
      endpoint.ServerName,
      _config.BackOffPeriodSeconds,
      backOffEndTime);
  }

  private async Task<StatsResponse?> GetStatsResponse(BitMeterEndPointConfig endpoint)
  {
    try
    {
      var url = endpoint.BuildUrl("getStats");
      var body = await _httpService.GetUrl(url);
      var parsedResponse = _responseService.ParseStatsResponse(endpoint, body);

      if (parsedResponse is null)
      {
        endpoint.ResponseParsingErrors += 1;
        return null;
      }

      endpoint.MissedPolls = 0;
      endpoint.BackOffEndTime = null;
      return parsedResponse;
    }
    catch (TaskCanceledException)
    {
      HandleBackOff(endpoint);

      _logger.LogWarning("Timed out after {time} ms getting stats from {server}",
        _config.HttpServiceTimeoutMs,
        endpoint.ServerName);
    }
    catch (Exception ex)
    {
      HandleBackOff(endpoint);
      _logger.LogError(ex, "{type}: {message}", ex.GetType().Name, ex.Message);
    }

    return null;
  }
}
