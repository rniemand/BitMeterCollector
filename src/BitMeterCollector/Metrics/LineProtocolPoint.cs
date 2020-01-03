using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BitMeterCollector.Metrics
{
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
}