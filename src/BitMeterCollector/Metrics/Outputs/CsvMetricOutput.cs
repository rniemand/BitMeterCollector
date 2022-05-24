using System.Collections.Generic;

namespace BitMeterCollector.Metrics.Outputs;

public class CsvMetricOutput : IMetricOutput
{
  public bool Enabled { get; } = false;

  public void SendMetrics(List<LineProtocolPoint> metrics)
  {
    // swallow
  }
}
