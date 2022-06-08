using System;
using BitMeterCollector.Shared.Configuration;

namespace BitMeterCollector.T1.Tests.TestSupport.Builders;

public class BitMeterEndPointConfigBuilder
{
  private readonly BitMeterEndPointConfig _endPoint = new();

  public BitMeterEndPointConfigBuilder WithEnabled(bool enabled)
  {
    _endPoint.Enabled = enabled;
    return this;
  }

  public BitMeterEndPointConfigBuilder WithBackOffEndTime(DateTime? backOffEndTime)
  {
    _endPoint.BackOffEndTime = backOffEndTime;
    return this;
  }

  public BitMeterEndPointConfig Build() => _endPoint;
}
