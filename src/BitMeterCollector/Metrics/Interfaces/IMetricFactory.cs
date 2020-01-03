using BitMeterCollector.Models;

namespace BitMeterCollector.Metrics.Interfaces
{
  public interface IMetricFactory
  {
    LineProtocolPoint FromStatsResponse(StatsResponse response);
  }
}