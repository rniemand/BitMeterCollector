using BitMeterCollector.Abstractions;
using BitMeterCollector.Configuration;
using BitMeterCollector.Metrics;
using BitMeterCollector.Metrics.Outputs;
using BitMeterCollector.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BitMeterCollector
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
      return Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
          // Build config
          var section = hostContext.Configuration.GetSection("BitMeter");
          var rootConfig = new BitMeterCollectorConfig();
          section.Bind(rootConfig);

          // Register services
          services
            .AddSingleton(rootConfig)
            .AddSingleton<IHttpService, HttpService>()
            .AddSingleton<IResponseParser, ResponseParser>()
            .AddSingleton<IDateTimeAbstraction, DateTimeAbstraction>()
            .AddSingleton<IMetricFactory, MetricFactory>()
            .AddSingleton<IBitMeterCollector, Services.BitMeterCollector>()
            .AddSingleton<IMetricService, MetricService>()
            .AddSingleton<IMetricOutput, RabbitMQMetricOutput>()
            .AddSingleton<IMetricOutput, CsvMetricOutput>()
            .AddHostedService<Worker>();
        });
    }
      
  }
}
