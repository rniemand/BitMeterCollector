using System;

namespace BitMeterCollector.Abstractions;

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
