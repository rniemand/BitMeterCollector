using System.Collections.Generic;
using BitMeterCollector.Shared.Configuration;
using Microsoft.Extensions.Configuration;

namespace BitMeterCollector.T1.Tests.TestSupport.Builders;

public class TestConfigurationBuilder
{
  public static IConfiguration Empty = new TestConfigurationBuilder().Build();

  private readonly Dictionary<string, string> _configuration = new();

  public TestConfigurationBuilder WithConfig(BitMeterConfig config)
  {
    _configuration["BitMeter:collectionIntervalSec"] = config.CollectionIntervalSec.ToString("D");
    _configuration["BitMeter:httpServiceTimeoutMs"] = config.HttpServiceTimeoutMs.ToString("D");
    _configuration["BitMeter:maxMissedPolls"] = config.MaxMissedPolls.ToString("D");
    _configuration["BitMeter:backOffPeriodSeconds"] = config.BackOffPeriodSeconds.ToString("D");

    var counter = 0;
    foreach (var server in config.Servers)
    {
      _configuration[$"BitMeter:servers:{counter}:name"] = server.ServerName;
      _configuration[$"BitMeter:servers:{counter}:useHttps"] = server.UseHttps ? "true" : "false";
      _configuration[$"BitMeter:servers:{counter}:enabled"] = server.Enabled ? "true" : "false";
      _configuration[$"BitMeter:servers:{counter}:ipAddress"] = server.IPAddress;
      _configuration[$"BitMeter:servers:{counter}:port"] = server.Port.ToString("D");
      counter++;
    }

    return this;
  }

  public IConfiguration Build() =>
    new ConfigurationBuilder()
      .AddInMemoryCollection(_configuration)
      .Build();
}
