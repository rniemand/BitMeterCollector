namespace BitMeterCollector.Shared.Models;

public class StatsResponse
{
  public long DownloadToday { get; set; }
  public long DownloadWeek { get; set; }
  public long DownloadMonth { get; set; }
  public long UploadToday { get; set; }
  public long UploadWeek { get; set; }
  public long UploadMonth { get; set; }
  public long TotalToday { get; set; }
  public long TotalWeek { get; set; }
  public long TotalMonth { get; set; }
  public string HostName { get; set; } = string.Empty;
}
