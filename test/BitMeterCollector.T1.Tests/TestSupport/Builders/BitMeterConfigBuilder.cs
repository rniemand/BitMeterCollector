using System;
using System.Collections.Generic;
using BitMeterCollector.Shared.Configuration;

namespace BitMeterCollector.T1.Tests.TestSupport.Builders;

public class BitMeterConfigBuilder
{
  public static BitMeterConfig Default => new BitMeterConfigBuilder().Build();

  private readonly BitMeterConfig _config = new();
  private readonly List<BitMeterEndPointConfig> _endPoints = new();

  public BitMeterConfigBuilder WithEndPoint(Func<BitMeterEndPointConfigBuilder, BitMeterEndPointConfigBuilder> builder)
  {
    _endPoints.Add(builder.Invoke(new BitMeterEndPointConfigBuilder()).Build());
    _config.Servers = _endPoints.ToArray();
    return this;
  }

  public BitMeterConfig Build() => _config;
}
