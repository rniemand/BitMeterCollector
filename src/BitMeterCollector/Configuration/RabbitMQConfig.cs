namespace BitMeterCollector.Configuration;

public class RabbitMQConfig
{
  public string UserName { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public string VirtualHost { get; set; } = string.Empty;
  public string HostName { get; set; } = string.Empty;
  public int Port { get; set; } = 5672;
  public bool Enabled { get; set; } = false;
  public string Exchange { get; set; } = string.Empty;
  public string RoutingKey { get; set; } = "metrics.bitmeter.stats";
  public int MaxAllowedSendFailures { get; set; } = 5;
  public int BackOffTimeSeconds { get; set; } = 60 * 5;
}
