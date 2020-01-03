using System.Threading;
using System.Threading.Tasks;
using BitMeterCollector.Configuration;
using BitMeterCollector.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitMeterCollector
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;
    private readonly BitMeterCollectorConfig _config;
    private readonly IBitMeterCollector _bitMeterCollector;

    public Worker(
      ILogger<Worker> logger,
      BitMeterCollectorConfig config,
      IBitMeterCollector bitMeterCollector)
    {
      _logger = logger;
      _config = config;
      _bitMeterCollector = bitMeterCollector;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        await _bitMeterCollector.Tick();
        await Task.Delay(_config.TickIntervalMs, stoppingToken);
      }
    }
  }
}
