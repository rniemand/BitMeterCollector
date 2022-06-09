using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Extensions;
using BitMeterCollector.Shared.Models;
using Rn.NetCore.Common.Logging;

namespace BitMeterCollector.Shared.Services;

public interface IResponseService
{
  StatsResponse? ParseStatsResponse(BitMeterEndPointConfig config, string rawResponse);
}

public class ResponseService : IResponseService
{
  private readonly ILoggerAdapter<ResponseService> _logger;

  public ResponseService(ILoggerAdapter<ResponseService> logger)
  {
    _logger = logger;
  }

  public StatsResponse? ParseStatsResponse(BitMeterEndPointConfig config, string rawResponse)
  {
    if (string.IsNullOrWhiteSpace(rawResponse))
    {
      _logger.LogError("Empty response from {server}", config.ServerName);
      return null;
    }

    // Ensure that we have our expected 6 entries
    var entries = rawResponse.Split(",", StringSplitOptions.RemoveEmptyEntries);
    if (entries.Length < 6)
    {
      _logger.LogError("Expecting 6 entries, got {count}", entries.Length);
      return null;
    }

    // Create and map the response object
    try
    {
      var parsed = new StatsResponse
      {
        DownloadToday = long.Parse(entries[0]),
        UploadToday = long.Parse(entries[1]),
        DownloadWeek = long.Parse(entries[2]),
        UploadWeek = long.Parse(entries[3]),
        DownloadMonth = long.Parse(entries[4]),
        UploadMonth = long.Parse(entries[5]),
        HostName = config.ServerName.LowerTrim()
      };

      // Calculate the totals
      parsed.TotalToday = parsed.DownloadToday + parsed.UploadToday;
      parsed.TotalWeek = parsed.DownloadWeek + parsed.UploadWeek;
      parsed.TotalMonth = parsed.DownloadMonth + parsed.UploadMonth;

      return parsed;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "{type}: {message} | {stack}",
        ex.GetType().Name,
        ex.Message,
        ex.HumanStackTrace());

      return null;
    }
  }
}
