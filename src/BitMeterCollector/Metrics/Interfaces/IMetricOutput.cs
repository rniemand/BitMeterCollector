using System.Collections.Generic;

namespace BitMeterCollector.Metrics.Interfaces
{
  public interface IMetricOutput
  {
    bool Enabled { get; }

    void SendMetrics(IEnumerable<LineProtocolPoint> metrics);
  }
}