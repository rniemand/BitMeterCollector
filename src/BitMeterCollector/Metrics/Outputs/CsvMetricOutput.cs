using System.Collections.Generic;

namespace BitMeterCollector.Metrics.Outputs
{
  public class CsvMetricOutput : IMetricOutput
  {
    public bool Enabled { get; }

    public void SendMetrics(IEnumerable<LineProtocolPoint> metrics)
    {
    }
  }
}