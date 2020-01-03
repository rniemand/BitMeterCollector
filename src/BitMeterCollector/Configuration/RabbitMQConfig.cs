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
  }
}