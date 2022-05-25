using System.Threading;
using System.Threading.Tasks;
using BitMeterCollector.Shared.Services;
using Microsoft.Extensions.Hosting;

namespace BitMeterCollector;

public class Worker : BackgroundService
{
  private readonly IBitMeterCollector _bitMeterCollector;

  public Worker(IBitMeterCollector bitMeterCollector)
  {
    _bitMeterCollector = bitMeterCollector;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      await _bitMeterCollector.TickAsync(stoppingToken);
    }
  }
}
