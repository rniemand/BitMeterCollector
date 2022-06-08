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

  public DateTime? BackOffEndTime { get; set; }
}
