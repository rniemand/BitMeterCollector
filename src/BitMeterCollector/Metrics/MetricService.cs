using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using BitMeterCollector.Metrics.Outputs;
using Microsoft.Extensions.Logging;

namespace BitMeterCollector.Metrics
{
  public interface IMetricService
  {
    void EnqueueMetric(LineProtocolPoint point);
  }

  public class MetricService : IMetricService
  {
    private readonly ILogger<MetricService> _logger;
    private readonly ConcurrentQueue<LineProtocolPoint> _metrics;
    private readonly Timer _flushTimer;
    private readonly List<IMetricOutput> _outputs;

    public MetricService(
      ILogger<MetricService> logger,
      IEnumerable<IMetricOutput> outputs)
    {
      _logger = logger;

      _flushTimer = new Timer(1000);
      _flushTimer.Elapsed += FlushMetrics;
      _flushTimer.Start();

      _metrics = new ConcurrentQueue<LineProtocolPoint>();
      _outputs = outputs.Where(o => o.Enabled).ToList();
    }

    public void EnqueueMetric(LineProtocolPoint point)
    {
      _metrics.Enqueue(point);
    }

    private void FlushMetrics(object sender, ElapsedEventArgs e)
    {
      if (_metrics.IsEmpty) return;

      _flushTimer.Stop();
      _logger.LogDebug($"Flushing {_metrics.Count} queued metrics");

      // Dequeue metrics to send
      var metrics = new List<LineProtocolPoint>();
      while (!_metrics.IsEmpty)
      {
        if (_metrics.TryDequeue(out var entry))
        {
          metrics.Add(entry);
        }
      }

      // Send metrics to each enabled output
      foreach (var output in _outputs)
      {
        output.SendMetrics(metrics);
      }

      _flushTimer.Start();
    }
  }
}