using System;
using BitMeterCollector.Shared.Configuration;

namespace BitMeterCollector.T1.Tests.TestSupport.Builders;

public class BitMeterEndPointConfigBuilder
{
  public static BitMeterEndPointConfig Default = new BitMeterEndPointConfigBuilder()
    .WithName("MyServer")
    .Build();

  private readonly BitMeterEndPointConfig _endPoint = new();

  public BitMeterEndPointConfigBuilder WithName(string name)
  {
    _endPoint.ServerName = name;
    return this;
  }

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

  public BitMeterEndPointConfigBuilder WithUseHttps(bool useHttps)
  {
    _endPoint.UseHttps = useHttps;
    return this;
  }

  public BitMeterEndPointConfigBuilder WithIPAddress(string ipAddress)
  {
    _endPoint.IPAddress = ipAddress;
    return this;
  }

  public BitMeterEndPointConfigBuilder WithIPAddress(string ipAddress, int port) =>
    WithIPAddress(ipAddress).WithPort(port);

  public BitMeterEndPointConfigBuilder WithIPAddress(string ipAddress, int port, bool useHttps) =>
    WithIPAddress(ipAddress, port).WithUseHttps(useHttps);

  public BitMeterEndPointConfigBuilder WithPort(int port)
  {
    _endPoint.Port = port;
    return this;
  }

  public BitMeterEndPointConfigBuilder WithMissedPolls(int missedPolls)
  {
    _endPoint.MissedPolls = missedPolls;
    return this;
  }

  public BitMeterEndPointConfigBuilder WithMaxMissedPolls(int maxMissedPolls)
  {
    _endPoint.MaxMissedPolls = maxMissedPolls;
    return this;
  }
  
  public BitMeterEndPointConfig Build() => _endPoint;
}
