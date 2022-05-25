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
}
