using System.Collections.Generic;

namespace BitMeterCollector.Metrics.Outputs
{
  public interface IMetricOutput
  {
    bool Enabled { get; }

    void SendMetrics(List<LineProtocolPoint> metrics);
  }
}