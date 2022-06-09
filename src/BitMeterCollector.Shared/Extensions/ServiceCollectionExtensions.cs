using BitMeterCollector.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Rn.NetCore.BasicHttp.Factories;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Metrics.Extensions;
using Rn.NetCore.Metrics.Rabbit.Extensions;

namespace BitMeterCollector.Shared.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddBitMeterCollector(this IServiceCollection services, IConfiguration configuration)
  {
    services
      .AddSingleton<IHttpService, HttpService>()
      .AddSingleton<IResponseService, ResponseService>()
      .AddSingleton<IBitMeterCollector, Services.BitMeterCollector>()

      .AddSingleton<IDateTimeAbstraction, DateTimeAbstraction>()
      .AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>))

      .AddRnMetricsBase(configuration)
      .AddRnMetricRabbitMQ()

      .AddLogging(loggingBuilder =>
      {
        loggingBuilder.ClearProviders();
        loggingBuilder.SetMinimumLevel(LogLevel.Trace);
        loggingBuilder.AddNLog(configuration);
      });

    services.TryAddSingleton<IHttpClientFactory, HttpClientFactory>();

    return services;
  }
}
