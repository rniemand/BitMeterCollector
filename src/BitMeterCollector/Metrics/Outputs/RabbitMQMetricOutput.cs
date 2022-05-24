using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BitMeterCollector.Abstractions;
using BitMeterCollector.Configuration;
using BitMeterCollector.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace BitMeterCollector.Metrics.Outputs;

public class RabbitMQMetricOutput : IMetricOutput
{
  public bool Enabled { get; }

  private readonly ILogger<RabbitMQMetricOutput> _logger;
  private readonly IDateTimeAbstraction _dateTime;
  private readonly BitMeterCollectorConfig _config;
  private ConnectionFactory _connectionFactory;
  private DateTime? _cooldownEndTime;
  private IConnection _connection;
  private IModel _channel;
  private int _sendFailures;


  // Constructor
  public RabbitMQMetricOutput(
    ILogger<RabbitMQMetricOutput> logger,
    IDateTimeAbstraction dateTime,
    BitMeterCollectorConfig config)
  {
    _logger = logger;
    _dateTime = dateTime;
    _config = config;

    Enabled = config.RabbitMQ.Enabled;
    _sendFailures = 0;

    CreateConnectionFactory();
    Connect();
  }


  // Interface methods
  public void SendMetrics(List<LineProtocolPoint> metrics)
  {
    if (!CanSendMetrics())
      return;

    if (!StillConnected())
      Reconnect();

    if (!StillConnected())
    {
      StartReconnectCooldown();
      return;
    }

    try
    {
      _logger.LogTrace("Sending {x} metrics to RabbitMQ", metrics.Count);

      _channel.BasicPublish(
        exchange: _config.RabbitMQ.Exchange,
        routingKey: _config.RabbitMQ.RoutingKey,
        basicProperties: null,
        body: Encoding.UTF8.GetBytes(GeneratePayload(metrics))
      );

      HandlePublishSuccess();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, ex.AsGenericError());
      HandlePublishFailure();
    }
  }


  // Internal methods
  private static string GeneratePayload(IEnumerable<LineProtocolPoint> entries)
  {
    using var sw = new StringWriter();
    foreach (var point in entries)
    {
      point.Format(sw);
      sw.Write("\n");
    }

    return sw.ToString().Trim();
  }

  private bool CanSendMetrics()
  {
    if (!Enabled)
      return false;

    if (!_cooldownEndTime.HasValue)
      return true;

    // ReSharper disable once InvertIf
    if (_cooldownEndTime.Value <= _dateTime.Now)
    {
      _logger.LogInformation("Cooldown has ended, attempting to reconnect");
      _cooldownEndTime = null;
      return true;
    }

    return false;
  }

  private void HandlePublishFailure()
  {
    _sendFailures += 1;

    if (_sendFailures <= _config.RabbitMQ.MaxAllowedSendFailures)
      return;

    StartReconnectCooldown();
    _sendFailures = 0;
  }

  private void HandlePublishSuccess()
  {
    _sendFailures = 0;
  }

  private void CreateConnectionFactory()
  {
    _logger.LogTrace("Creating new connection factory");

    _connectionFactory = new ConnectionFactory
    {
      UserName = _config.RabbitMQ.UserName,
      Password = _config.RabbitMQ.Password,
      VirtualHost = _config.RabbitMQ.VirtualHost,
      HostName = _config.RabbitMQ.HostName,
      Port = _config.RabbitMQ.Port
    };
  }

  private void Connect()
  {
    if (!Enabled)
      return;

    try
    {
      _connection = _connectionFactory.CreateConnection();
      _channel = _connection.CreateModel();
      _logger.LogInformation("Connected to RabbitMQ");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, ex.AsGenericError());
    }
  }

  private bool StillConnected()
  {
    if (_channel == null || _connection == null)
      return false;

    if (_channel.IsClosed || !_channel.IsOpen)
      return false;

    // ReSharper disable once ConvertIfStatementToReturnStatement
    if (!_connection.IsOpen)
      return false;

    return true;
  }

  private void TearDownConnection()
  {
    // Check if there is anything to tear down
    if (_channel == null && _connection == null)
      return;

    try
    {
      _logger.LogInformation("Tearing down RabbitMQ connection");

      // Close and dispose the channel
      if (_channel != null)
      {
        _logger.LogDebug("Attempting to close / dispose channel");
        _channel?.Close();
        _channel?.Dispose();
      }

      // Close and dispose the connection
      if (_connection == null)
        return;

      _logger.LogDebug("Attempting to close / dispose connection");
      _connection?.Close();
      _connection?.Dispose();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, ex.AsGenericError());
    }
    finally
    {
      _channel = null;
      _connection = null;
    }
  }

  private void Reconnect()
  {
    TearDownConnection();
    Connect();
  }

  private void StartReconnectCooldown()
  {
    var cooldownTimeSec = _config.RabbitMQ.BackOffTimeSeconds;
    _cooldownEndTime = _dateTime.Now.AddSeconds(cooldownTimeSec);

    _logger.LogWarning(
      "Entering cooldown period until {endTime}",
      _cooldownEndTime.Value
    );
  }
}
