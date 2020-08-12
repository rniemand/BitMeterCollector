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

    public BitMeterCollectorConfig()
    {
      // TODO: [TESTS] (BitMeterCollectorConfig.BitMeterCollectorConfig) Add tests

      HttpServiceTimeoutMs = 2000;
      MaxMissedPolls = 5;
      BackOffPeriodSeconds = 60 * 10;
    }
  }
}