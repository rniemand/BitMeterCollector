using System;
using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Extensions;
using BitMeterCollector.T1.Tests.TestSupport.Builders;
using NUnit.Framework;

namespace BitMeterCollector.T1.Tests.Extensions;

[TestFixture]
public class ConfigurationExtensionsTests
{
  [Test]
  public void BindBitMeterConfig_GivenNoConfigSection_ShouldReturnDefaultConfig()
  {
    // arrange
    var configuration = TestConfigurationBuilder.Empty;

    // act
    var boundConfig = configuration.BindBitMeterConfig();

    // assert
    Assert.That(boundConfig, Is.Not.Null);
    AssertAreEqual(new BitMeterConfig(), boundConfig);
  }

  [Test]
  public void BindBitMeterConfig_GivenConfigSection_ShouldReturnBoundConfig()
  {
    // arrange
    var config = new BitMeterConfigBuilder()
      .WithCollectionIntervalSec(100)
      .WithHttpServiceTimeoutMs(12)
      .WithMaxMissedPolls(8)
      .WithBackOffPeriodSeconds(88)
      .WithEndPoint(builder => builder
        .WithName("MyServer")
        .WithIPAddress("127.0.0.1", 9876, true)
        .WithEnabled(true))
      .WithEndPoint(builder => builder
        .WithName("MyServer")
        .WithIPAddress("192.168.0.60", 9876, true)
        .WithEnabled(true))
      .Build();

    var configuration = new TestConfigurationBuilder()
      .WithConfig(config)
      .Build();

    // act
    var boundConfig = configuration.BindBitMeterConfig();

    // assert
    Assert.That(boundConfig, Is.Not.Null);
    AssertAreEqual(config, boundConfig);
  }

  [Test]
  public void CanCollectStats_GivenServerNotInCoolDown_ShouldReturnTrue()
  {
    // arrange
    var currentDate = DateTime.Now;

    var endPointConfig = new BitMeterEndPointConfigBuilder()
      .WithBackOffEndTime(null)
      .Build();

    // act
    var canCollectStats = endPointConfig.CanCollectStats(currentDate);

    // assert
    Assert.That(canCollectStats, Is.True);
  }

  [Test]
  public void CanCollectStats_GivenServerInCoolDown_ShouldReturnFalse()
  {
    // arrange
    var currentDate = DateTime.Now;

    var endPointConfig = new BitMeterEndPointConfigBuilder()
      .WithBackOffEndTime(currentDate.AddSeconds(10))
      .Build();

    // act
    var canCollectStats = endPointConfig.CanCollectStats(currentDate);

    // assert
    Assert.That(canCollectStats, Is.False);
  }

  [Test]
  public void CanCollectStats_GivenServerCoolDownPassed_ShouldReturnTrue()
  {
    // arrange
    var currentDate = DateTime.Now;

    var endPointConfig = new BitMeterEndPointConfigBuilder()
      .WithBackOffEndTime(currentDate.AddSeconds(-5))
      .WithMissedPolls(10)
      .Build();

    // act
    Assert.That(endPointConfig.BackOffEndTime, Is.Not.Null);
    Assert.That(endPointConfig.MissedPolls, Is.EqualTo(10));
    var canCollectStats = endPointConfig.CanCollectStats(currentDate);

    // assert
    Assert.That(canCollectStats, Is.True);
    Assert.That(endPointConfig.BackOffEndTime, Is.Null);
    Assert.That(endPointConfig.MissedPolls, Is.EqualTo(0));
  }

  private static void AssertAreEqual(BitMeterConfig expected, BitMeterConfig actual)
  {
    AssertServersEqual(expected, actual);
    Assert.That(expected.CollectionIntervalSec, Is.EqualTo(actual.CollectionIntervalSec));
    Assert.That(expected.HttpServiceTimeoutMs, Is.EqualTo(actual.HttpServiceTimeoutMs));
    Assert.That(expected.MaxMissedPolls, Is.EqualTo(actual.MaxMissedPolls));
    Assert.That(expected.BackOffPeriodSeconds, Is.EqualTo(actual.BackOffPeriodSeconds));
  }

  private static void AssertAreEqual(BitMeterEndPointConfig expected, BitMeterEndPointConfig actual)
  {
    Assert.That(expected.UseHttps, Is.EqualTo(actual.UseHttps));
    Assert.That(expected.IPAddress, Is.EqualTo(actual.IPAddress));
    Assert.That(expected.Port, Is.EqualTo(actual.Port));
    Assert.That(expected.ServerName, Is.EqualTo(actual.ServerName));
    Assert.That(expected.Enabled, Is.EqualTo(actual.Enabled));
  }

  private static void AssertServersEqual(BitMeterConfig expected, BitMeterConfig actual)
  {
    if (expected.Servers.Length != actual.Servers.Length)
      Assert.Fail("Server collection are not the same length");

    for (var i = 0; i < expected.Servers.Length; i++)
      AssertAreEqual(expected.Servers[i], actual.Servers[i]);
  }
}
