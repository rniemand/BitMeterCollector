using System;
using BitMeterCollector.Shared.Abstractions;
using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Metrics;
using BitMeterCollector.Shared.Metrics.Outputs;
using BitMeterCollector.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BitMeterCollector;

public class Program
{
  public static void Main(string[] args)
  {
    var logger = LogManager
      .LoadConfiguration("nlog.config")
      .GetCurrentClassLogger();

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
          .AddSingleton<IResponseService, ResponseService>()
          .AddSingleton<IDateTimeAbstraction, DateTimeAbstraction>()
          .AddSingleton<IMetricFactory, MetricFactory>()
          .AddSingleton<IBitMeterCollector, Shared.Services.BitMeterCollector>()
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
