namespace BitMeterCollector.Shared.Extensions;

public static class StringExtensions
{
  public static string LowerTrim(this string input)
  {
    return input.ToLower().Trim();
  }
}
