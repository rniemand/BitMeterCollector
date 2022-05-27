using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Extensions;
using BitMeterCollector.Shared.Models;
using Microsoft.Extensions.Logging;

namespace BitMeterCollector.Shared.Services;

public interface IResponseService
{
  bool TryParseStatsResponse(BitMeterEndPointConfig config, string rawResponse, out StatsResponse parsed);
}

public class ResponseService : IResponseService
{
  private readonly ILogger<ResponseService> _logger;

  public ResponseService(ILogger<ResponseService> logger)
  {
    _logger = logger;
  }

  public bool TryParseStatsResponse(BitMeterEndPointConfig config, string rawResponse, out StatsResponse parsed)
  {
    parsed = new StatsResponse();

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
      _logger.LogError("Expecting 6 entries, got {count}", entries.Length);
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
      HostName = config.ServerName.LowerTrim()
    };

    // Calculate the totals
    parsed.TotalToday = parsed.DownloadToday + parsed.UploadToday;
    parsed.TotalWeek = parsed.DownloadWeek + parsed.UploadWeek;
    parsed.TotalMonth = parsed.DownloadMonth + parsed.UploadMonth;

    return true;
  }
}
