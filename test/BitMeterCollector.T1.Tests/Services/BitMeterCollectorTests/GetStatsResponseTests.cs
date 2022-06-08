using System;
using System.Threading;
using System.Threading.Tasks;
using BitMeterCollector.Shared.Models;
using BitMeterCollector.Shared.Services;
using BitMeterCollector.T1.Tests.TestSupport.Builders;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Rn.NetCore.Common.Logging;

namespace BitMeterCollector.T1.Tests.Services.BitMeterCollectorTests;

[TestFixture]
public class GetStatsResponseTests
{
  private const string IPAddress = "127.0.0.1";
  private const int Port = 8912;
  private const int DefaultTimeoutMs = 500;
  private const string ServerName = "MyServer";
  private const string BadServiceResponse = "BadServiceResponse";

  [Test]
  public async Task GetStatsResponse_GivenCalled_ShouldBuildExpectedUrl()
  {
    // arrange
    var httpService = Substitute.For<IHttpService>();

    var config = new BitMeterConfigBuilder()
      .WithEndPoint(builder => builder
        .WithEnabled(true)
        .WithIPAddress(IPAddress, Port, false))
      .Build();

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService);

    // act
    await collector.TickAsync(CancellationToken.None);

    // assert
    await httpService.Received(1).GetUrl("http://127.0.0.1:8912/getStats");
  }

  [Test]
  public async Task GetStatsResponse_GivenExceptionThrown_ShouldLog()
  {
    // arrange
    var logger = Substitute.For<ILoggerAdapter<Shared.Services.BitMeterCollector>>();
    var httpService = Substitute.For<IHttpService>();
    var ex = new Exception("Whoops");

    var config = new BitMeterConfigBuilder()
      .WithEndPoint(builder => builder
        .WithEnabled(true)
        .WithIPAddress(IPAddress, Port, false))
      .Build();

    httpService
      .GetUrl("http://127.0.0.1:8912/getStats")
      .Throws(ex);

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService,
      logger: logger);

    // act
    await collector.TickAsync(CancellationToken.None);

    // assert
    logger.Received(1).LogError(ex, "{type}: {message} | {stack}",
      ex.GetType().Name,
      ex.Message,
      ex.HumanStackTrace());
  }

  [Test]
  public async Task GetStatsResponse_GivenRequestTimedOut_ShouldLog()
  {
    // arrange
    var logger = Substitute.For<ILoggerAdapter<Shared.Services.BitMeterCollector>>();
    var httpService = Substitute.For<IHttpService>();
    var ex = new TaskCanceledException("Whoops");

    var config = new BitMeterConfigBuilder()
      .WithHttpServiceTimeoutMs(DefaultTimeoutMs)
      .WithEndPoint(builder => builder
        .WithEnabled(true)
        .WithIPAddress(IPAddress, Port, false)
        .WithName(ServerName))
      .Build();

    httpService
      .GetUrl("http://127.0.0.1:8912/getStats")
      .Throws(ex);

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService,
      logger: logger);

    // act
    await collector.TickAsync(CancellationToken.None);

    // assert
    logger.Received(1).LogWarning("Timed out after {time} ms getting stats from {server}",
      DefaultTimeoutMs,
      ServerName);
  }

  [Test]
  public async Task GetStatsResponse_GivenResponseParsingFails_ShouldIncrementResponseParsingErrors()
  {
    // arrange
    var httpService = Substitute.For<IHttpService>();
    var responseService = Substitute.For<IResponseService>();

    var endPointConfig = new BitMeterEndPointConfigBuilder()
      .WithEnabled(true)
      .WithIPAddress(IPAddress, Port, false)
      .WithName(ServerName)
      .Build();

    var config = new BitMeterConfigBuilder()
      .WithHttpServiceTimeoutMs(DefaultTimeoutMs)
      .WithEndPoint(endPointConfig)
      .Build();

    httpService
      .GetUrl("http://127.0.0.1:8912/getStats")
      .Returns(BadServiceResponse);

    responseService
      .ParseStatsResponse(endPointConfig, BadServiceResponse)
      .ReturnsNull();

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService,
      responseService: responseService);

    // act
    Assert.That(config.Servers[0].ResponseParsingErrors, Is.EqualTo(0));
    await collector.TickAsync(CancellationToken.None);

    // assert
    Assert.That(config.Servers[0].ResponseParsingErrors, Is.EqualTo(1));
  }

  [Test]
  public async Task GetStatsResponse_GivenResponseParses_ShouldResetMissedPolls()
  {
    // arrange
    var httpService = Substitute.For<IHttpService>();
    var responseService = Substitute.For<IResponseService>();

    var endPointConfig = new BitMeterEndPointConfigBuilder()
      .WithEnabled(true)
      .WithIPAddress(IPAddress, Port, false)
      .WithName(ServerName)
      .WithMissedPolls(1)
      .Build();

    var config = new BitMeterConfigBuilder()
      .WithHttpServiceTimeoutMs(DefaultTimeoutMs)
      .WithEndPoint(endPointConfig)
      .Build();

    httpService
      .GetUrl("http://127.0.0.1:8912/getStats")
      .Returns(BadServiceResponse);

    responseService
      .ParseStatsResponse(endPointConfig, BadServiceResponse)
      .Returns(new StatsResponse());

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService,
      responseService: responseService);

    // act
    Assert.That(config.Servers[0].MissedPolls, Is.EqualTo(1));
    await collector.TickAsync(CancellationToken.None);

    // assert
    Assert.That(config.Servers[0].MissedPolls, Is.EqualTo(0));
  }
}
