using BitMeterCollector.Shared.Models;
using Microsoft.Extensions.Logging;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Metrics.Models;

namespace BitMeterCollector.Shared.Factories;

public interface IMetricFactory
{
  CoreMetric FromStatsResponse(StatsResponse response);
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

  public CoreMetric FromStatsResponse(StatsResponse response)
  {
    var metric=new CoreMetric("bitmeter.stats")
      .SetTag("host", response.Hostname);

    metric.Fields["download_today"]= response.DownloadToday;
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
