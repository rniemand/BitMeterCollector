namespace BitMeterCollector.Shared.Configuration;

public class BitMeterCollectorConfig
{
  public RabbitMQConfig RabbitMQ { get; set; } = new();
  public BitMeterEndPointConfig[] Servers { get; set; } = Array.Empty<BitMeterEndPointConfig>();
  public int CollectionIntervalSec { get; set; } = 10;
  public int HttpServiceTimeoutMs { get; set; } = 750;
  public int MaxMissedPolls { get; set; } = 5;
  public int BackOffPeriodSeconds { get; set; } = 60 * 10;
  public bool LogMetricFlushing { get; set; }
  public int MetricFlushIntervalMs { get; set; } = 1000;
}
