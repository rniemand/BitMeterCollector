using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using BitMeterCollector.Configuration;
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
    private readonly bool _logMetricFlushing;

    public MetricService(
      ILogger<MetricService> logger,
      IEnumerable<IMetricOutput> outputs,
      BitMeterCollectorConfig config)
    {
      _logger = logger;

      _logMetricFlushing = config.LogMetricFlushing;
      _flushTimer = new Timer(config.MetricFlushIntervalMs);
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
      
      if (_logMetricFlushing)
        _logger.LogTrace("Flushing {count} queued metrics", _metrics.Count);

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