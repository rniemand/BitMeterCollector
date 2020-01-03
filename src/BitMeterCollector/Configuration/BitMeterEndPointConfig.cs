namespace BitMeterCollector.Configuration
{
  public class BitMeterEndPointConfig
  {
    public bool UseHttps { get; set; }
    public string IPAddress { get; set; }
    public int Port { get; set; }
    public string ServerName { get; set; }
    public bool Enabled { get; set; }

    public string BuildUrl(string append = null)
    {
      var baseUrl = "http";
      baseUrl += UseHttps ? "s" : "";
      baseUrl += $"://{IPAddress}:{Port}";

      if (!string.IsNullOrEmpty(append))
      {
        baseUrl += $"/{append}";
      }

      return baseUrl;
    }
  }
}