using System;
using BitMeterCollector.Abstractions.Interfaces;

namespace BitMeterCollector.Abstractions
{
  public class DateTimeAbstraction : IDateTimeAbstraction
  {
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
  }
}