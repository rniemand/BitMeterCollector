using BitMeterCollector.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using RnCore.Abstractions;
using RnCore.Logging;
using RnCore.Metrics.Extensions;
using RnCore.Metrics.InfluxDb;

namespace BitMeterCollector.Shared.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddBitMeterCollector(this IServiceCollection services, IConfiguration configuration) =>
    services
      .AddSingleton<IHttpService, HttpService>()
      .AddSingleton<IResponseService, ResponseService>()
      .AddSingleton<IBitMeterCollector, Services.BitMeterCollector>()

      .AddSingleton<IDateTimeAbstraction, DateTimeAbstraction>()
      .AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>))
      .AddRnCoreMetrics()
      .AddInfluxDbMetricOutput()

      .AddLogging(loggingBuilder =>
      {
        loggingBuilder.ClearProviders();
        loggingBuilder.SetMinimumLevel(LogLevel.Trace);
        loggingBuilder.AddNLog(configuration);
      });
}
