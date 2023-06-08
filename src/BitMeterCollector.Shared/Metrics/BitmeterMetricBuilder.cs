using BitMeterCollector.Shared.Models;
using RnCore.Metrics.Builders;

namespace BitMeterCollector.Shared.Metrics;

internal class BitmeterMetricBuilder : BaseMetricBuilder<BitmeterMetricBuilder>
{
  public BitmeterMetricBuilder()
    : base("stats")
  {
    SetSuccess(true);
  }

  public BitmeterMetricBuilder(StatsResponse response)
    : this()
  {
    ForResponse(response);
  }
  
  public BitmeterMetricBuilder ForResponse(StatsResponse response)
  {
    AddAction(m => m.SetTag("host", response.HostName));
    AddAction(m => m.SetField("download_today", response.DownloadToday));
    AddAction(m => m.SetField("download_week", response.DownloadWeek));
    AddAction(m => m.SetField("download_month", response.DownloadMonth));
    AddAction(m => m.SetField("upload_today", response.UploadToday));
    AddAction(m => m.SetField("upload_week", response.UploadWeek));
    AddAction(m => m.SetField("upload_month", response.UploadMonth));
    AddAction(m => m.SetField("total_today", response.TotalToday));
    AddAction(m => m.SetField("total_week", response.TotalWeek));
    AddAction(m => m.SetField("total_month", response.TotalMonth));
    
    return this;
  }
}
