using System;
using System.Threading;
using System.Threading.Tasks;
using BitMeterCollector.Shared.Extensions;
using BitMeterCollector.Shared.Services;
using BitMeterCollector.T1.Tests.TestSupport.Builders;
using NSubstitute;
using NUnit.Framework;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Metrics;

namespace BitMeterCollector.T1.Tests.Services.BitMeterCollectorTests;

[TestFixture]
public class TickAsyncTests
{
  private const string IPAddress = "127.0.0.1";
  private const int Port = 8912;
  private const int DefaultTimeoutMs = 500;
  private const string ServerName = "MyServer";
  private const string GoodServiceResponse = "GoodServiceResponse";
  private const int BackOffPeriodSeconds = 10;

  [Test]
  public async Task TickAsync_GivenCollectionSucceeded_ShouldLogMetric()
  {
    // arrange
    var httpService = Substitute.For<IHttpService>();
    var responseService = Substitute.For<IResponseService>();
    var metricService = Substitute.For<IMetricService>();
    var dateTime = Substitute.For<IDateTimeAbstraction>();
    var statsResponse = StatsResponseBuilder.Default;
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
      .Returns(GoodServiceResponse);

    responseService
      .ParseStatsResponse(endPointConfig, GoodServiceResponse)
      .Returns(statsResponse);

    var collector = TestHelper.GetBitMeterCollector(
      config: config,
      httpService: httpService,
      responseService: responseService,
      dateTime: dateTime,
      metricService: metricService);

    // act
    await collector.TickAsync(CancellationToken.None);

    // assert
    await metricService.Received(1).SubmitAsync(Arg.Is<CoreMetric>(m =>
      m.Tags["host"] == statsResponse.HostName.LowerTrim() &&
      (long)m.Fields["download_today"] == statsResponse.DownloadToday &&
      (long)m.Fields["download_week"] == statsResponse.DownloadWeek &&
      (long)m.Fields["download_month"] == statsResponse.DownloadMonth &&

      (long)m.Fields["upload_today"] == statsResponse.UploadToday &&
      (long)m.Fields["upload_week"] == statsResponse.UploadWeek &&
      (long)m.Fields["upload_month"] == statsResponse.UploadMonth &&

      (long)m.Fields["total_today"] == statsResponse.TotalToday &&
      (long)m.Fields["total_week"] == statsResponse.TotalWeek &&
      (long)m.Fields["total_month"] == statsResponse.TotalMonth));
  }
}
