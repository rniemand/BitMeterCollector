using BitMeterCollector.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BitMeterCollector;

public class Program
{
  public static void Main(string[] args)
  {
    CreateHostBuilder(args).Build().Run();
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
          .AddBitMeterCollector(hostContext.Configuration)
          .AddHostedService<Worker>();
      });
  }
}
