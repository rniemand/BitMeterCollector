namespace BitMeterCollector.Configuration
{
  public class BitMeterCollectorConfig
  {
    public RabbitMQConfig RabbitMQ { get; set; }
    public BitMeterEndPointConfig[] Servers { get; set; }
    public int TickIntervalMs { get; set; }
  }
}