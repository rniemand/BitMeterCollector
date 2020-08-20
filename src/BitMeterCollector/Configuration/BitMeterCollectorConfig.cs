namespace BitMeterCollector.Configuration
{
  public class BitMeterCollectorConfig
  {
    public RabbitMQConfig RabbitMQ { get; set; }
    public BitMeterEndPointConfig[] Servers { get; set; }
    public int CollectionIntervalSec { get; set; }
    public int HttpServiceTimeoutMs { get; set; }
    public int MaxMissedPolls { get; set; }
    public int BackOffPeriodSeconds { get; set; }
    public bool LogMetricFlushing { get; set; }
    public int MetricFlushIntervalMs { get; set; }

    public BitMeterCollectorConfig()
    {
      // TODO: [TESTS] (BitMeterCollectorConfig.BitMeterCollectorConfig) Add tests

      RabbitMQ = new RabbitMQConfig();
      Servers = new BitMeterEndPointConfig[0];
      CollectionIntervalSec = 10;
      HttpServiceTimeoutMs = 750;
      MaxMissedPolls = 5;
      BackOffPeriodSeconds = 60 * 10;
      LogMetricFlushing = false;
      MetricFlushIntervalMs = 1000;
    }
  }
}