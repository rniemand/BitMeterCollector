using System;
using BitMeterCollector.Shared.Abstractions;
using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Extensions;
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
        var bitmeterConfig = hostContext.Configuration.BindBitMeterConfig();

        // Ensure all servers have the "max missed polls" value set
        foreach (var server in bitmeterConfig.Servers)
          server.SetMaxMissedPolls(bitmeterConfig.MaxMissedPolls);

        // Register services
        services
          .AddSingleton(bitmeterConfig)
          .AddBitMeterCollector()
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
