namespace BitMeterCollector.Shared.Metrics.Outputs;

public interface IMetricOutput
{
  bool Enabled { get; }

  void SendMetrics(List<LineProtocolPoint> metrics);
}
