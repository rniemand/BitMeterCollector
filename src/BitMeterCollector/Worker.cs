using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace BitMeterCollector
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;
    private readonly BitMeterCollectorConfig _config;
    private readonly IBitMeterCollector _bitMeterCollector;

    public Worker(
      ILogger<Worker> logger,
      BitMeterCollectorConfig config,
      IBitMeterCollector bitMeterCollector)
    {
      _logger = logger;
      _config = config;
      _bitMeterCollector = bitMeterCollector;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        await _bitMeterCollector.Tick();
        await Task.Delay(_config.TickIntervalMs, stoppingToken);
      }
    }
  }

  public class BitMeterCollectorConfig
  {
    public RabbitMQConfig RabbitMQ { get; set; }
    public BitMeterEndPointConfig[] Servers { get; set; }
    public int TickIntervalMs { get; set; }
  }

  public class BitMeterEndPointConfig
  {
    public bool UseHttps { get; set; }
    public string IPAddress { get; set; }
    public int Port { get; set; }
    public string ServerName { get; set; }
    public bool Enabled { get; set; }

    public string BuildUrl(string append = null)
    {
      var baseUrl = "http";
      baseUrl += UseHttps ? "s" : "";
      baseUrl += $"://{IPAddress}:{Port}";

      if (!string.IsNullOrEmpty(append))
      {
        baseUrl += $"/{append}";
      }

      return baseUrl;
    }
  }

  public class RabbitMQConfig
  {
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; }
    public string HostName { get; set; }
    public int Port { get; set; }
    public bool Enabled { get; set; }
    public string Exchange { get; set; }
    public string RoutingKey { get; set; }
  }

  public static class DateUtils
  {
    public static long GetUtcTime()
    {
      var foo = DateTime.Now;
      var dtOffset = ((DateTimeOffset)foo);
      var unixTime = dtOffset.ToUnixTimeSeconds();
      var secondsOff = dtOffset.Offset.TotalSeconds;

      if (secondsOff < 0)
      {
        unixTime -= ((long)secondsOff * -1);
      }

      if (secondsOff > 0)
      {
        unixTime += (long)secondsOff;
      }

      return unixTime;
    }
  }

  public interface IHttpService
  {
    Task<string> GetUrl(string url);
  }

  public class HttpService : IHttpService
  {
    private readonly HttpClient _httpClient;

    public HttpService()
    {
      _httpClient = new HttpClient();
    }

    public async Task<string> GetUrl(string url)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, url);
      var response = await _httpClient.SendAsync(request);
      var responseBody = await response.Content.ReadAsStringAsync();

      return responseBody;
    }
  }

  public class StatsResponse
  {
    public long DownloadToday { get; set; }
    public long DownloadWeek { get; set; }
    public long DownloadMonth { get; set; }
    public long UploadToday { get; set; }
    public long UploadWeek { get; set; }
    public long UploadMonth { get; set; }
    public long TotalToday { get; set; }
    public long TotalWeek { get; set; }
    public long TotalMonth { get; set; }
    public string Hostname { get; set; }
  }

  public static class StringExtensions
  {
    public static string LowerTrim(this string input)
    {
      return input.ToLower().Trim();
    }
  }

  public interface IResponseParser
  {
    bool TryParseStatsResponse(BitMeterEndPointConfig config, string rawResponse, out StatsResponse parsed);
  }

  public class ResponseParser : IResponseParser
  {
    private readonly ILogger<ResponseParser> _logger;

    public ResponseParser(ILogger<ResponseParser> logger)
    {
      _logger = logger;
    }

    public bool TryParseStatsResponse(BitMeterEndPointConfig config, string rawResponse, out StatsResponse parsed)
    {
      parsed = null;

      // Ensure this is not an empty string
      if (string.IsNullOrWhiteSpace(rawResponse))
      {
        _logger.LogError("Empty response provided");
        return false;
      }

      // Ensure that we have our expected 6 entries
      var entries = rawResponse.Split(",", StringSplitOptions.RemoveEmptyEntries);
      if (entries.Length < 6)
      {
        _logger.LogError($"Expecting 6 entries, got {entries.Length}");
        return false;
      }

      // Create and map the response object
      parsed = new StatsResponse
      {
        DownloadToday = long.Parse(entries[0]),
        UploadToday = long.Parse(entries[1]),
        DownloadWeek = long.Parse(entries[2]),
        UploadWeek = long.Parse(entries[3]),
        DownloadMonth = long.Parse(entries[4]),
        UploadMonth = long.Parse(entries[5]),
        Hostname = config.ServerName.LowerTrim()
      };

      // Calculate the totals
      parsed.TotalToday = parsed.DownloadToday + parsed.UploadToday;
      parsed.TotalWeek = parsed.DownloadWeek + parsed.UploadWeek;
      parsed.TotalMonth = parsed.DownloadMonth + parsed.UploadMonth;

      return true;
    }
  }

  public static class LineProtocolSyntax
  {
    // FROM: https://github.com/influxdata/influxdb-csharp

    private static readonly DateTime Origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly Dictionary<Type, Func<object, string>> Formatters =
      new Dictionary<Type, Func<object, string>>
      {
        {typeof(sbyte), FormatInteger},
        {typeof(byte), FormatInteger},
        {typeof(short), FormatInteger},
        {typeof(ushort), FormatInteger},
        {typeof(int), FormatInteger},
        {typeof(uint), FormatInteger},
        {typeof(long), FormatInteger},
        {typeof(ulong), FormatInteger},
        {typeof(float), FormatFloat},
        {typeof(double), FormatFloat},
        {typeof(decimal), FormatFloat},
        {typeof(bool), FormatBoolean},
        {typeof(TimeSpan), FormatTimespan}
      };

    public static string EscapeName(string nameOrKey)
    {
      if (nameOrKey == null) throw new ArgumentNullException(nameof(nameOrKey));
      return nameOrKey
        .Replace("=", "\\=")
        .Replace(" ", "\\ ")
        .Replace(",", "\\,");
    }

    public static string FormatValue(object value)
    {
      var v = value ?? "";

      if (Formatters.TryGetValue(v.GetType(), out var format))
        return format(v);

      return FormatString(v.ToString());
    }

    public static string FormatTimestamp(DateTime utcTimestamp)
    {
      var t = utcTimestamp - Origin;
      return (t.Ticks * 100L).ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatInteger(object i)
    {
      return ((IFormattable)i).ToString(null, CultureInfo.InvariantCulture) + "i";
    }

    private static string FormatFloat(object f)
    {
      return ((IFormattable)f).ToString(null, CultureInfo.InvariantCulture);
    }

    private static string FormatTimespan(object ts)
    {
      return ((TimeSpan)ts).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatBoolean(object b)
    {
      return ((bool)b) ? "t" : "f";
    }

    private static string FormatString(string s)
    {
      return "\"" + s.Replace("\"", "\\\"") + "\"";
    }
  }

  public class LineProtocolPoint
  {
    // FROM: https://github.com/influxdata/influxdb-csharp

    public string Measurement { get; }
    public IReadOnlyDictionary<string, object> Fields { get; }
    public IReadOnlyDictionary<string, string> Tags { get; }
    public DateTime? UtcTimestamp { get; }

    public LineProtocolPoint(
        string measurement,
        IReadOnlyDictionary<string, object> fields,
        IReadOnlyDictionary<string, string> tags = null,
        DateTime? utcTimestamp = null)
    {
      // RunTick validation on provided values
      ValidateMeasurement(measurement);
      ValidateFields(fields);
      ValidateTags(tags);
      ValidateTimeStamp(utcTimestamp);

      // Assign values
      Measurement = measurement;
      Fields = fields;
      Tags = tags;
      UtcTimestamp = utcTimestamp;
    }

    public void Format(TextWriter textWriter)
    {
      if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

      textWriter.Write(LineProtocolSyntax.EscapeName(Measurement));

      if (Tags != null)
      {
        foreach (var t in Tags.OrderBy(t => t.Key))
        {
          if (string.IsNullOrEmpty(t.Value))
            continue;

          textWriter.Write(',');
          textWriter.Write(LineProtocolSyntax.EscapeName(t.Key));
          textWriter.Write('=');
          textWriter.Write(LineProtocolSyntax.EscapeName(t.Value));
        }
      }

      var fieldDelim = ' ';
      foreach (var f in Fields)
      {
        textWriter.Write(fieldDelim);
        fieldDelim = ',';
        textWriter.Write(LineProtocolSyntax.EscapeName(f.Key));
        textWriter.Write('=');
        textWriter.Write(LineProtocolSyntax.FormatValue(f.Value));
      }

      if (UtcTimestamp != null)
      {
        textWriter.Write(' ');
        textWriter.Write(LineProtocolSyntax.FormatTimestamp(UtcTimestamp.Value));
      }
    }


    // Validation Methods
    private static void ValidateMeasurement(string measurement)
    {
      if (string.IsNullOrEmpty(measurement))
      {
        throw new ArgumentException("A measurement name must be specified");
      }
    }

    private static void ValidateFields(IReadOnlyDictionary<string, object> fields)
    {
      if (fields == null || fields.Count == 0)
      {
        throw new ArgumentException("At least one field must be specified");
      }

      // ReSharper disable once LoopCanBeConvertedToQuery
      foreach (var f in fields)
      {
        if (string.IsNullOrEmpty(f.Key))
        {
          throw new ArgumentException("Fields must have non-empty names");
        }
      }
    }

    private static void ValidateTags(IReadOnlyDictionary<string, string> tags)
    {
      if (tags == null)
        return;

      // ReSharper disable once LoopCanBeConvertedToQuery
      foreach (var t in tags)
      {
        if (string.IsNullOrEmpty(t.Key))
        {
          throw new ArgumentException("Tags must have non-empty names");
        }
      }
    }

    private static void ValidateTimeStamp(DateTime? utcTimestamp)
    {
      if (utcTimestamp != null && utcTimestamp.Value.Kind != DateTimeKind.Utc)
      {
        throw new ArgumentException("Timestamps must be specified as UTC");
      }
    }
  }

  public interface IDateTimeAbstraction
  {
    DateTime Now { get; }
    DateTime UtcNow { get; }
  }

  public class DateTimeAbstraction : IDateTimeAbstraction
  {
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
  }

  public interface IMetricFactory
  {
    LineProtocolPoint FromStatsResponse(StatsResponse response);
  }

  public class MetricFactory : IMetricFactory
  {
    private readonly ILogger<MetricFactory> _logger;
    private readonly IDateTimeAbstraction _dateTime;

    public MetricFactory(
      ILogger<MetricFactory> logger,
      IDateTimeAbstraction dateTime)
    {
      _logger = logger;
      _dateTime = dateTime;
    }

    public LineProtocolPoint FromStatsResponse(StatsResponse response)
    {
      return new LineProtocolPointBuilder()
        .ForMeasurement("bitmeter.stats")
        .WithTag("host", response.Hostname)
        .WithField("download_today", response.DownloadToday)
        .WithField("download_week", response.DownloadWeek)
        .WithField("download_month", response.DownloadMonth)
        .WithField("upload_today", response.UploadToday)
        .WithField("upload_week", response.UploadWeek)
        .WithField("upload_month", response.UploadMonth)
        .WithField("total_today", response.TotalToday)
        .WithField("total_week", response.TotalWeek)
        .WithField("total_month", response.TotalMonth)
        .Build(_dateTime.UtcNow);
    }
  }

  public class LineProtocolPointBuilder
  {
    private string _measurement;
    private readonly Dictionary<string, object> _fields;
    private readonly Dictionary<string, string> _tags;

    public LineProtocolPointBuilder()
    {
      _measurement = "metrics.unset";
      _fields = new Dictionary<string, object>();
      _tags = new Dictionary<string, string>();
    }

    public LineProtocolPointBuilder ForMeasurement(string measurement)
    {
      _measurement = measurement;
      return this;
    }

    public LineProtocolPointBuilder WithTag(string tag, string value)
    {
      _tags[tag] = value;
      return this;
    }

    public LineProtocolPointBuilder WithField(string field, long value)
    {
      _fields[field] = value;
      return this;
    }

    public LineProtocolPoint Build(DateTime timeStamp)
    {
      return new LineProtocolPoint(_measurement, _fields, _tags, timeStamp);
    }
  }

  public interface IBitMeterCollector
  {
    Task Tick();
  }

  public interface IMetricService
  {
    void EnqueueMetric(LineProtocolPoint point);
  }

  public interface IMetricOutput
  {
    bool Enabled { get; }

    void SendMetrics(IEnumerable<LineProtocolPoint> metrics);
  }

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

  public class CsvMetricOutput : IMetricOutput
  {
    public bool Enabled { get; }

    public void SendMetrics(IEnumerable<LineProtocolPoint> metrics)
    {
    }
  }

  public class MetricService : IMetricService
  {
    private readonly ILogger<MetricService> _logger;
    private readonly ConcurrentQueue<LineProtocolPoint> _metrics;
    private readonly System.Timers.Timer _flushTimer;
    private readonly List<IMetricOutput> _outputs;

    public MetricService(
      ILogger<MetricService> logger,
      IEnumerable<IMetricOutput> outputs)
    {
      _logger = logger;

      _flushTimer = new System.Timers.Timer(1000);
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

  public class BitMeterCollector : IBitMeterCollector
  {
    private readonly ILogger<BitMeterCollector> _logger;
    private readonly BitMeterCollectorConfig _config;
    private readonly IHttpService _httpService;
    private readonly IResponseParser _responseParser;
    private readonly IMetricFactory _metricFactory;
    private readonly IMetricService _metricService;

    public BitMeterCollector(
      ILogger<BitMeterCollector> logger,
      BitMeterCollectorConfig config,
      IHttpService httpService,
      IResponseParser responseParser,
      IMetricFactory metricFactory,
      IMetricService metricService)
    {
      _logger = logger;
      _config = config;
      _httpService = httpService;
      _responseParser = responseParser;
      _metricFactory = metricFactory;
      _metricService = metricService;
    }

    public async Task Tick()
    {
      var servers = _config.Servers.Where(s => s.Enabled).ToList();

      foreach (var server in servers)
      {
        var url = server.BuildUrl("getStats");
        var body = await _httpService.GetUrl(url);

        if (_responseParser.TryParseStatsResponse(server, body, out var parsed))
        {
          _metricService.EnqueueMetric(_metricFactory.FromStatsResponse(parsed));
        }
      }


    }
  }
}
