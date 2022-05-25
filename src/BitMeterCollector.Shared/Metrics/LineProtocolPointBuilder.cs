namespace BitMeterCollector.Shared.Metrics;

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
