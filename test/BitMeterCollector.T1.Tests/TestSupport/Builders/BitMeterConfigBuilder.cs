using BitMeterCollector.Shared.Configuration;

namespace BitMeterCollector.T1.Tests.TestSupport.Builders;

public class BitMeterConfigBuilder
{
  public static BitMeterConfig Default => new BitMeterConfigBuilder().Build();

  private readonly BitMeterConfig _config = new();

  public BitMeterConfig Build() => _config;
}
