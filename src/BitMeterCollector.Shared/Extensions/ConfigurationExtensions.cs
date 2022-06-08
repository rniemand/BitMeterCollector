using BitMeterCollector.Shared.Configuration;
using Microsoft.Extensions.Configuration;

namespace BitMeterCollector.Shared.Extensions;

public static class ConfigurationExtensions
{
  public static BitMeterConfig BindBitMeterConfig(this IConfiguration configuration)
  {
    var section = configuration.GetSection("BitMeter");
    var boundConfig = new BitMeterConfig();

    if (section.Exists())
      section.Bind(boundConfig);

    return boundConfig;
  }

  public static string BuildUrl(this BitMeterEndPointConfig config, string? append = null)
  {
    var baseUrl = "http";
    baseUrl += config.UseHttps ? "s" : "";
    baseUrl += $"://{config.IPAddress}:{config.Port}";

    if (!string.IsNullOrEmpty(append))
    {
      baseUrl += $"/{append}";
    }

    return baseUrl;
  }

  public static bool CanCollectStats(this BitMeterEndPointConfig config, DateTime now)
  {
    // Not in a cool-off period
    if (!config.BackOffEndTime.HasValue)
      return true;

    // Waiting for cool-off period to end
    if (config.BackOffEndTime.Value > now)
      return false;

    // Cool-off period has ended
    config.BackOffEndTime = null;
    config.MissedPolls = 0;
    return true;
  }
}
