using System.Text;

namespace BitMeterCollector.Shared.Extensions;

public static class ExceptionExtensions
{
  public static string HumanStackTrace(this Exception ex)
  {
    var sb = new StringBuilder();
    WalkException(sb, ex, 1);
    return sb.ToString();
  }

  public static string AsGenericError(this Exception ex)
  {
    var exType = ex.GetType().Name;
    return $"An unexpected {exType} was thrown: {ex.Message}. | {ex.HumanStackTrace()}";
  }

  // Internal methods
  private static void WalkException(StringBuilder sb, Exception ex, int level)
  {
    sb.Append(level == 1 ? "" : "    ")
      .Append(level)
      .Append($" ({ex.GetType().FullName}) ")
      .Append(ex.Message)
      .AppendLine();

    if (ex.InnerException != null)
    {
      WalkException(sb, ex.InnerException, level + 1);
    }
  }
}
