using System.Threading;
using System.Threading.Tasks;
using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitMeterCollector;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private readonly BitMeterConfig _config;
  private readonly IBitMeterCollector _bitMeterCollector;

  public Worker(
    ILogger<Worker> logger,
    BitMeterConfig config,
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
      await Task.Delay(_config.CollectionIntervalSec * 1000, stoppingToken);
    }
  }
}
