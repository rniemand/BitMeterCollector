using System;
using BitMeterCollector.Configuration;
using BitMeterCollector.Extensions;
using BitMeterCollector.Models;
using Microsoft.Extensions.Logging;

namespace BitMeterCollector.Services
{
  public interface IResponseParser
  {
    // TODO: [RENAME] (IResponseParser.IResponseParser) Rename to ResponseService
    bool TryParseStatsResponse(BitMeterEndPointConfig config, string rawResponse, out StatsResponse parsed);
  }

  public class ResponseParser : IResponseParser
  {
    private readonly ILogger<ResponseParser> _logger;

    public ResponseParser(ILogger<ResponseParser> logger)
    {
      _logger = logger;
    }

    public bool TryParseStatsResponse(BitMeterEndPointConfig config, string rawResponse, out StatsResponse parsed)
    {
      parsed = null;

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
        _logger.LogError($"Expecting 6 entries, got {entries.Length}");
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
        Hostname = config.ServerName.LowerTrim()
      };

      // Calculate the totals
      parsed.TotalToday = parsed.DownloadToday + parsed.UploadToday;
      parsed.TotalWeek = parsed.DownloadWeek + parsed.UploadWeek;
      parsed.TotalMonth = parsed.DownloadMonth + parsed.UploadMonth;

      return true;
    }
  }
}