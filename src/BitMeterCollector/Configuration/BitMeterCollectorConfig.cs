namespace BitMeterCollector.Configuration
{
  public class BitMeterCollectorConfig
  {
    public RabbitMQConfig RabbitMQ { get; set; }
    public BitMeterEndPointConfig[] Servers { get; set; }
    public int CollectionIntervalSec { get; set; }
    public int HttpServiceTimeoutMs { get; set; }

    public BitMeterCollectorConfig()
    {
      HttpServiceTimeoutMs = 2000;
    }
  }
}