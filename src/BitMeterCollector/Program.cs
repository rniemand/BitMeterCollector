using System;
using BitMeterCollector.Abstractions;
using BitMeterCollector.Configuration;
using BitMeterCollector.Metrics;
using BitMeterCollector.Metrics.Outputs;
using BitMeterCollector.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BitMeterCollector
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var logger = LogManager.GetCurrentClassLogger();

      try
      {
        logger.Info("Starting BitMeterCollector");
        CreateHostBuilder(args).Build().Run();
      }
      catch (Exception ex)
      {
        logger.Error(ex, "Stopping BitMeterCollector because of an exception");
        throw;
      }
      finally
      {
        LogManager.Shutdown();
      }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
      return Host
        .CreateDefaultBuilder(args)
        .UseWindowsService()
        .ConfigureServices((hostContext, services) =>
        {
          // TODO: [TESTS] (Program.CreateHostBuilder) Add tests

          // Build config
          var section = hostContext.Configuration.GetSection("BitMeter");
          var rootConfig = new BitMeterCollectorConfig();
          section.Bind(rootConfig);

          // Ensure all servers have the "max missed polls" value set
          foreach (var server in rootConfig.Servers)
            server.SetMaxMissedPolls(rootConfig.MaxMissedPolls);

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
            .AddLogging(loggingBuilder =>
            {
              // configure Logging with NLog
              loggingBuilder.ClearProviders();
              loggingBuilder.SetMinimumLevel(LogLevel.Trace);
              loggingBuilder.AddNLog(hostContext.Configuration);
            })
            .AddHostedService<Worker>();
        });
    }
  }
}
