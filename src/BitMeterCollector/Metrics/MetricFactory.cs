using BitMeterCollector.Abstractions;
using BitMeterCollector.Models;
using Microsoft.Extensions.Logging;

namespace BitMeterCollector.Metrics;

public interface IMetricFactory
{
  LineProtocolPoint FromStatsResponse(StatsResponse response);
}

public class MetricFactory : IMetricFactory
{
  private readonly ILogger<MetricFactory> _logger;
  private readonly IDateTimeAbstraction _dateTime;

  public MetricFactory(
    ILogger<MetricFactory> logger,
    IDateTimeAbstraction dateTime)
  {
    _logger = logger;
    _dateTime = dateTime;
  }

  public LineProtocolPoint FromStatsResponse(StatsResponse response)
  {
    return new LineProtocolPointBuilder()
      .ForMeasurement("bitmeter.stats")
      .WithTag("host", response.Hostname)
      .WithField("download_today", response.DownloadToday)
      .WithField("download_week", response.DownloadWeek)
      .WithField("download_month", response.DownloadMonth)
      .WithField("upload_today", response.UploadToday)
      .WithField("upload_week", response.UploadWeek)
      .WithField("upload_month", response.UploadMonth)
      .WithField("total_today", response.TotalToday)
      .WithField("total_week", response.TotalWeek)
      .WithField("total_month", response.TotalMonth)
      .Build(_dateTime.UtcNow);
  }
}
