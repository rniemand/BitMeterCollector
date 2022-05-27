using Microsoft.Extensions.Configuration;

namespace BitMeterCollector.Shared.Configuration;

// DOCS: docs\config\BitMeterConfig.md
public class BitMeterConfig
{
  [ConfigurationKeyName("servers")]
  public BitMeterEndPointConfig[] Servers { get; set; } = Array.Empty<BitMeterEndPointConfig>();

  [ConfigurationKeyName("collectionIntervalSec")]
  public int CollectionIntervalSec { get; set; } = 10;

  [ConfigurationKeyName("httpServiceTimeoutMs")]
  public int HttpServiceTimeoutMs { get; set; } = 750;

  [ConfigurationKeyName("maxMissedPolls")]
  public int MaxMissedPolls { get; set; } = 5;

  [ConfigurationKeyName("backOffPeriodSeconds")]
  public int BackOffPeriodSeconds { get; set; } = 60 * 10;
}
