namespace BitMeterCollector.Metrics.Interfaces
{
  public interface IMetricService
  {
    void EnqueueMetric(LineProtocolPoint point);
  }
}