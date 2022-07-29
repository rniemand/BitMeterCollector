using System;
using System.Threading;
using System.Threading.Tasks;
using BitMeterCollector.Shared.Services;
using BitMeterCollector.T1.Tests.TestSupport.Builders;
using NSubstitute;
using NUnit.Framework;
using Rn.NetCore.Common.Abstractions;

namespace BitMeterCollector.T1.Tests.Services.BitMeterCollectorTests;

[TestFixture]
public class GetServersTests
{
  [Test]
  public async Task GetServers_GivenNoServers_ShouldReturnEmptyCollection()
  {
    // arrange
    var httpService = Substitute.For<IHttpService>();
    var config = BitMeterConfigBuilder.Default;

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService);

    // act
    await collector.TickAsync(CancellationToken.None);

    // assert
    await httpService.DidNotReceive().GetUrl(Arg.Any<string>());
  }

  [Test]
  public async Task GetServers_GivenNoEnabledServers_ShouldReturnEmptyCollection()
  {
    // arrange
    var httpService = Substitute.For<IHttpService>();
    var config = new BitMeterConfigBuilder()
      .WithCollectionIntervalSec(0)
      .WithEndPoint(builder => builder.WithEnabled(false))
      .Build();

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService);

    // act
    await collector.TickAsync(CancellationToken.None);

    // assert
    await httpService.DidNotReceive().GetUrl(Arg.Any<string>());
  }

  [Test]
  public async Task GetServers_GivenEnabledServerNotTimeToCollect_ShouldReturnEmptyCollection()
  {
    // arrange
    var httpService = Substitute.For<IHttpService>();
    var dateTime = Substitute.For<IDateTimeAbstraction>();
    var baseDate = DateTime.Now;

    var config = new BitMeterConfigBuilder()
      .WithCollectionIntervalSec(0)
      .WithEndPoint(builder => builder
        .WithEnabled(true)
        .WithBackOffEndTime(baseDate.AddMinutes(1)))
      .Build();

    dateTime.Now.Returns(baseDate);

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService,
      dateTime: dateTime);

    // act
    await collector.TickAsync(CancellationToken.None);

    // assert
    await httpService.DidNotReceive().GetUrl(Arg.Any<string>());
  }
}
