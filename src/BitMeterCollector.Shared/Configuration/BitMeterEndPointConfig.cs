using Microsoft.Extensions.Configuration;

namespace BitMeterCollector.Shared.Configuration;

// DOCS: docs\config\BitMeterEndPointConfig.md
public class BitMeterEndPointConfig
{
  [ConfigurationKeyName("useHttps")]
  public bool UseHttps { get; set; } = false;

  [ConfigurationKeyName("ipAddress")]
  public string IPAddress { get; set; } = string.Empty;

  [ConfigurationKeyName("port")]
  public int Port { get; set; } = 9876;

  [ConfigurationKeyName("name")]
  public string ServerName { get; set; } = string.Empty;

  [ConfigurationKeyName("enabled")]
  public bool Enabled { get; set; } = true;

  public int MissedPolls { get; set; }
  public int MaxMissedPolls { get; set; } = 5;
  public DateTime? BackOffEndTime { get; private set; }

  public string BuildUrl(string? append = null)
  {
    var baseUrl = "http";
    baseUrl += UseHttps ? "s" : "";
    baseUrl += $"://{IPAddress}:{Port}";

    if (!string.IsNullOrEmpty(append))
    {
      baseUrl += $"/{append}";
    }

    return baseUrl;
  }

  public bool UnsuccessfulPoll()
  {
    MissedPolls += 1;
    return MissedPolls >= MaxMissedPolls;
  }

  public void SuccessfulPoll()
  {
    MissedPolls = 0;
    BackOffEndTime = null;
  }

  public void SetMaxMissedPolls(int amount)
  {
    MaxMissedPolls = amount;
  }

  public void SetBackOffEndTime(DateTime endTime)
  {
    BackOffEndTime = endTime;
  }

  public bool CanCollectStats(DateTime now)
  {
    // Not in a cool-off period
    if (!BackOffEndTime.HasValue)
      return true;

    // Waiting for cool-off period to end
    if (BackOffEndTime.Value > now)
      return false;

    // Cool-off period has ended
    BackOffEndTime = null;
    MissedPolls = 0;
    return true;
  }
}
