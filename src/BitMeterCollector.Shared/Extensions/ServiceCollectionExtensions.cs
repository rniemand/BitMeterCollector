using BitMeterCollector.Shared.Factories;
using BitMeterCollector.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Metrics;
using Rn.NetCore.Metrics.Outputs;
using Rn.NetCore.Metrics.Rabbit;
using Rn.NetCore.Metrics.Rabbit.Interfaces;

namespace BitMeterCollector.Shared.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddBitMeterCollector(this IServiceCollection services)
  {
    return services
      .AddSingleton<IHttpService, HttpService>()
      .AddSingleton<IResponseService, ResponseService>()
      .AddSingleton<IMetricFactory, MetricFactory>()
      .AddSingleton<IBitMeterCollector, Services.BitMeterCollector>()

      .AddSingleton<IDateTimeAbstraction, DateTimeAbstraction>()
      .AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>))

      .AddSingleton<IMetricServiceUtils, MetricServiceUtils>()
      .AddSingleton<IMetricService, MetricService>()
      .AddSingleton<IRabbitFactory, RabbitFactory>()
      .AddSingleton<IRabbitConnection, RabbitConnection>()
      .AddSingleton<IMetricOutput, RabbitMetricOutput>();
  }
}
