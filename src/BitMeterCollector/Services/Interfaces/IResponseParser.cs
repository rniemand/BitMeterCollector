using BitMeterCollector.Configuration;
using BitMeterCollector.Models;

namespace BitMeterCollector.Services.Interfaces
{
  public interface IResponseParser
  {
    // TODO: [RENAME] (IResponseParser.IResponseParser) Rename to ResponseService
    bool TryParseStatsResponse(BitMeterEndPointConfig config, string rawResponse, out StatsResponse parsed);
  }
}