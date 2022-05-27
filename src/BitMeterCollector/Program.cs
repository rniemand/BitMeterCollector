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
        var bmConfig = hostContext.Configuration.BindBitMeterConfig();

        // Ensure all servers have the "max missed polls" value set
        foreach (var server in bmConfig.Servers)
          server.SetMaxMissedPolls(bmConfig.MaxMissedPolls);

        // Register services
        services
          .AddSingleton(bmConfig)
          .AddBitMeterCollector(hostContext.Configuration)
          .AddHostedService<Worker>();
      });
  }
}
