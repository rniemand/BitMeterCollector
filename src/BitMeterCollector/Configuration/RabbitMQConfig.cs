namespace BitMeterCollector.Configuration
{
  public class RabbitMQConfig
  {
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; }
    public string HostName { get; set; }
    public int Port { get; set; }
    public bool Enabled { get; set; }
    public string Exchange { get; set; }
    public string RoutingKey { get; set; }
    public int MaxAllowedSendFailures { get; set; }
    public int BackOffTimeSeconds { get; set; }

    public RabbitMQConfig()
    {
      // TODO: [TESTS] (RabbitMQConfig.RabbitMQConfig) Add tests

      UserName = string.Empty;
      Password = string.Empty;
      VirtualHost = string.Empty;
      HostName = string.Empty;
      Port = 5672;
      Enabled = false;
      Exchange = string.Empty;
      RoutingKey = "metrics.bitmeter.stats";
      MaxAllowedSendFailures = 5;
      BackOffTimeSeconds = 60 * 5;
    }
  }
}