using System;
using System.Threading;
using System.Threading.Tasks;
using BitMeterCollector.Shared.Services;
using BitMeterCollector.T1.Tests.TestSupport.Builders;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;

namespace BitMeterCollector.T1.Tests.Services.BitMeterCollectorTests;

[TestFixture]
public class HandleBackOffTests
{
  private const string IPAddress = "127.0.0.1";
  private const int Port = 8912;
  private const int DefaultTimeoutMs = 500;
  private const string ServerName = "MyServer";
  private const string BadServiceResponse = "BadServiceResponse";
  private const int BackOffPeriodSeconds = 10;

  [Test]
  public async Task HandleBackOff_GivenExceedsMaxMissedPolls_ShouldSetBackOffEndTime()
  {
    // arrange
    var httpService = Substitute.For<IHttpService>();
    var dateTime = Substitute.For<IDateTimeAbstraction>();
    var ex = new Exception("Whoops");
    var baseDate = DateTime.Now;

    var endPointConfig = new BitMeterEndPointConfigBuilder()
      .WithEnabled(true)
      .WithIPAddress(IPAddress, Port, false)
      .WithName(ServerName)
      .WithMaxMissedPolls(1)
      .WithMissedPolls(1)
      .Build();

    var config = new BitMeterConfigBuilder()
      .WithCollectionIntervalSec(0)
      .WithHttpServiceTimeoutMs(DefaultTimeoutMs)
      .WithBackOffPeriodSeconds(BackOffPeriodSeconds)
      .WithEndPoint(endPointConfig)
      .Build();

    dateTime.Now.Returns(baseDate);

    httpService
      .GetUrl("http://127.0.0.1:8912/getStats")
      .Throws(ex);

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService,
      dateTime: dateTime);

    // act
    Assert.That(endPointConfig.BackOffEndTime, Is.Null);
    await collector.TickAsync(CancellationToken.None);

    // assert
    Assert.That(endPointConfig.BackOffEndTime, Is.EqualTo(baseDate.AddSeconds(BackOffPeriodSeconds)));
  }

  [Test]
  public async Task HandleBackOff_GivenExceedsMaxMissedPolls_ShouldLog()
  {
    // arrange
    var logger = Substitute.For<ILoggerAdapter<Shared.Services.BitMeterCollector>>();
    var httpService = Substitute.For<IHttpService>();
    var dateTime = Substitute.For<IDateTimeAbstraction>();
    var ex = new Exception("Whoops");
    var baseDate = DateTime.Now;

    var endPointConfig = new BitMeterEndPointConfigBuilder()
      .WithEnabled(true)
      .WithIPAddress(IPAddress, Port, false)
      .WithName(ServerName)
      .WithMaxMissedPolls(1)
      .WithMissedPolls(2)
      .Build();

    var config = new BitMeterConfigBuilder()
      .WithCollectionIntervalSec(0)
      .WithHttpServiceTimeoutMs(DefaultTimeoutMs)
      .WithBackOffPeriodSeconds(BackOffPeriodSeconds)
      .WithEndPoint(endPointConfig)
      .Build();

    dateTime.Now.Returns(baseDate);

    httpService
      .GetUrl("http://127.0.0.1:8912/getStats")
      .Throws(ex);

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService,
      dateTime: dateTime,
      logger: logger);

    // act
    await collector.TickAsync(CancellationToken.None);

    // assert
    logger.Received(1).LogInformation(
      "Unable to reach {server} - backing off for {time} seconds (will try again at {date})",
      ServerName,
      BackOffPeriodSeconds,
      baseDate.AddSeconds(BackOffPeriodSeconds));
  }
}
