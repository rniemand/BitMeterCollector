using BitMeterCollector.Shared.Abstractions;
using BitMeterCollector.Shared.Metrics;
using BitMeterCollector.Shared.Metrics.Outputs;
using BitMeterCollector.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BitMeterCollector.Shared.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddBitMeterCollector(this IServiceCollection services)
  {
    return services
      .AddSingleton<IHttpService, HttpService>()
      .AddSingleton<IResponseService, ResponseService>()
      .AddSingleton<IDateTimeAbstraction, DateTimeAbstraction>()
      .AddSingleton<IMetricFactory, MetricFactory>()
      .AddSingleton<IBitMeterCollector, Services.BitMeterCollector>()
      .AddSingleton<IMetricService, MetricService>()
      .AddSingleton<IMetricOutput, RabbitMQMetricOutput>()
      .AddSingleton<IMetricOutput, CsvMetricOutput>();
  }
}
