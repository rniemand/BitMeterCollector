using System.Collections.Generic;
using System.IO;
using System.Text;
using BitMeterCollector.Configuration;
using BitMeterCollector.Metrics.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace BitMeterCollector.Metrics.Outputs
{
  public class RabbitMQMetricOutput : IMetricOutput
  {
    public bool Enabled { get; }

    private readonly ILogger<RabbitMQMetricOutput> _logger;
    private readonly BitMeterCollectorConfig _config;
    private IConnection _connection;
    private IModel _channel;

    public RabbitMQMetricOutput(
      ILogger<RabbitMQMetricOutput> logger,
      BitMeterCollectorConfig config)
    {
      _logger = logger;
      _config = config;

      Enabled = config.RabbitMQ.Enabled;
      Connect();
    }

    public void SendMetrics(IEnumerable<LineProtocolPoint> metrics)
    {
      // TODO: [COMPLETE] (RabbitMQMetricOutput.SendMetrics) Complete me
      // ensure we are connected

      var rabbitPayload = GeneratePayload(metrics);

      _channel.BasicPublish(
        exchange: _config.RabbitMQ.Exchange,
        routingKey: _config.RabbitMQ.RoutingKey,
        basicProperties: null,
        body: Encoding.UTF8.GetBytes(rabbitPayload)
      );
    }

    private static string GeneratePayload(IEnumerable<LineProtocolPoint> entries)
    {
      using (var sw = new StringWriter())
      {
        foreach (var point in entries)
        {
          point.Format(sw);
          sw.Write("\n");
        }

        return sw.ToString().Trim();
      }
    }

    private void Connect()
    {
      if (!Enabled) return;

      // TODO: [LOGGING] (RabbitMQMetricOutput.Connect) Add logging

      var conFactory = new ConnectionFactory
      {
        UserName = _config.RabbitMQ.UserName,
        Password = _config.RabbitMQ.Password,
        VirtualHost = _config.RabbitMQ.VirtualHost,
        HostName = _config.RabbitMQ.HostName,
        Port = _config.RabbitMQ.Port
      };

      _connection = conFactory.CreateConnection();
      _channel = _connection.CreateModel();
    }
  }
}