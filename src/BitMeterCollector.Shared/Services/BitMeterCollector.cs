using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Extensions;
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
  private readonly IMetricService _metricService;
  private readonly IDateTimeAbstraction _dateTime;

  public BitMeterCollector(
    ILogger<BitMeterCollector> logger,
    BitMeterConfig config,
    IHttpService httpService,
    IResponseService responseService,
    IMetricService metricService,
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
      await _metricService.SubmitAsync(CreateMetric(response));
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

  private void HandleServerBackOff(BitMeterEndPointConfig endpoint)
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
    var url = endpoint.BuildUrl("getStats");
    var mustBackOff = false;

    try
    {
      var body = await _httpService.GetUrl(url);
      if (_responseService.TryParseStatsResponse(endpoint, body, out var parsed))
      {
        endpoint.MissedPolls = 0;
        endpoint.BackOffEndTime = null;
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
      _logger.LogError(ex, "{type}: {message}. | {stack}",
        ex.GetType().Name,
        ex.Message,
        ex.HumanStackTrace());
    }
    finally
    {
      if (mustBackOff) HandleServerBackOff(endpoint);
    }

    return null;
  }

  private static CoreMetric CreateMetric(StatsResponse response)
  {
    var metric = new CoreMetric("bitmeter.stats")
      .SetTag("host", response.HostName);

    metric.Fields["download_today"] = response.DownloadToday;
    metric.Fields["download_week"] = response.DownloadWeek;
    metric.Fields["download_month"] = response.DownloadMonth;
    metric.Fields["upload_today"] = response.UploadToday;
    metric.Fields["upload_week"] = response.UploadWeek;
    metric.Fields["upload_month"] = response.UploadMonth;
    metric.Fields["total_today"] = response.TotalToday;
    metric.Fields["total_week"] = response.TotalWeek;
    metric.Fields["total_month"] = response.TotalMonth;

    return metric;
  }
}
