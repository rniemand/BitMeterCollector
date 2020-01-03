using System;

namespace BitMeterCollector.Abstractions.Interfaces
{
  public interface IDateTimeAbstraction
  {
    DateTime Now { get; }
    DateTime UtcNow { get; }
  }
}