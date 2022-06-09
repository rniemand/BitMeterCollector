using System;
using System.Collections.Generic;
using BitMeterCollector.Shared.Configuration;

namespace BitMeterCollector.T1.Tests.TestSupport.Builders;

public class BitMeterConfigBuilder
{
  public static BitMeterConfig Default => new BitMeterConfigBuilder()
    .WithCollectionIntervalSec(0)
    .Build();

  private readonly BitMeterConfig _config = new();
  private readonly List<BitMeterEndPointConfig> _endPoints = new();

  public BitMeterConfigBuilder WithEndPoint(Func<BitMeterEndPointConfigBuilder, BitMeterEndPointConfigBuilder> builder)
  {
    _endPoints.Add(builder.Invoke(new BitMeterEndPointConfigBuilder()).Build());
    _config.Servers = _endPoints.ToArray();
    return this;
  }

  public BitMeterConfigBuilder WithEndPoint(BitMeterEndPointConfig endPoint)
  {
    _endPoints.Add(endPoint);
    _config.Servers = _endPoints.ToArray();
    return this;
  }

  public BitMeterConfigBuilder WithHttpServiceTimeoutMs(int timeoutMs)
  {
    _config.HttpServiceTimeoutMs = timeoutMs;
    return this;
  }

  public BitMeterConfigBuilder WithBackOffPeriodSeconds(int seconds)
  {
    _config.BackOffPeriodSeconds = seconds;
    return this;
  }

  public BitMeterConfigBuilder WithCollectionIntervalSec(int interval)
  {
    _config.CollectionIntervalSec = interval;
    return this;
  }

  public BitMeterConfig Build() => _config;
}
