using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Metrics;

namespace BitMeterCollector.T1.Tests.Services.BitMeterCollectorTests;

public static class TestHelper
{
  public static Shared.Services.BitMeterCollector GetBitMeterCollector(
    ILogger<Shared.Services.BitMeterCollector>? logger = null,
    BitMeterConfig? config = null,
    IHttpService? httpService = null,
    IResponseService? responseService = null,
    IMetricService? metricService = null,
    IDateTimeAbstraction? dateTime = null) =>
    new(
      logger ?? Substitute.For<ILogger<Shared.Services.BitMeterCollector>>(),
      config ?? new BitMeterConfig(),
      httpService ?? Substitute.For<IHttpService>(),
      responseService ?? Substitute.For<IResponseService>(),
      metricService ?? Substitute.For<IMetricService>(),
      dateTime ?? new DateTimeAbstraction());
}
