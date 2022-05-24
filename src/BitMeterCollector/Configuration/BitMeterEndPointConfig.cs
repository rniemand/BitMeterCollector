using System;

namespace BitMeterCollector.Configuration;

public class BitMeterEndPointConfig
{
  public bool UseHttps { get; set; } = false;
  public string IPAddress { get; set; } = string.Empty;
  public int Port { get; set; } = 9876;
  public string ServerName { get; set; } = string.Empty;
  public bool Enabled { get; set; } = false;
  public int MissedPolls { get; set; }
  public int MaxMissedPolls { get; set; } = 5;
  public DateTime? BackOffEndTime { get; private set; }

  public string BuildUrl(string append = null)
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
    // TODO: [TESTS] (BitMeterEndPointConfig.UnsuccessfulPoll) Add tests

    MissedPolls += 1;

    return MissedPolls >= MaxMissedPolls;
  }

  public void SuccessfulPoll()
  {
    // TODO: [TESTS] (BitMeterEndPointConfig.SuccessfulPoll) Add tests

    MissedPolls = 0;
    BackOffEndTime = null;
  }

  public void SetMaxMissedPolls(int amount)
  {
    // TODO: [TESTS] (BitMeterEndPointConfig.SetMaxMissedPolls) Add tests

    MaxMissedPolls = amount;
  }

  public void SetBackOffEndTime(DateTime endTime)
  {
    // TODO: [TESTS] (BitMeterEndPointConfig.SetBackOffEndTime) Add tests

    BackOffEndTime = endTime;
  }

  public bool CanCollectStats(DateTime now)
  {
    // TODO: [TESTS] (BitMeterEndPointConfig.CanCollectStats) Add tests

    // Not enabled
    if (Enabled == false)
      return false;
      
    // Not in a cool-off period
    if (BackOffEndTime.HasValue == false)
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
