namespace BitMeterCollector.Shared.Utilities;

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
