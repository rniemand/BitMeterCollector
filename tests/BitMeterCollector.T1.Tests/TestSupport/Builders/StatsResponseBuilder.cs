using BitMeterCollector.Shared.Models;

namespace BitMeterCollector.T1.Tests.TestSupport.Builders;

public class StatsResponseBuilder
{
  public static StatsResponse Default = new StatsResponseBuilder().WithValidDefaults().Build();

  private readonly StatsResponse _response = new();

  public StatsResponseBuilder WithValidDefaults() =>
    WithDownloadToday(10)
      .WithDownloadWeek(11)
      .WithDownloadMonth(12)
      .WithUploadToday(13)
      .WithUploadWeek(14)
      .WithUploadMonth(15)
      .WithTotalToday(16)
      .WithTotalWeek(17)
      .WithTotalMonth(18)
      .WithHostName("MyHost");

  public StatsResponseBuilder WithDownloadToday(long value)
  {
    _response.DownloadToday = value;
    return this;
  }

  public StatsResponseBuilder WithDownloadWeek(long value)
  {
    _response.DownloadWeek = value;
    return this;
  }

  public StatsResponseBuilder WithDownloadMonth(long value)
  {
    _response.DownloadMonth = value;
    return this;
  }

  public StatsResponseBuilder WithUploadToday(long value)
  {
    _response.UploadToday = value;
    return this;
  }

  public StatsResponseBuilder WithUploadWeek(long value)
  {
    _response.UploadWeek = value;
    return this;
  }

  public StatsResponseBuilder WithUploadMonth(long value)
  {
    _response.UploadMonth = value;
    return this;
  }

  public StatsResponseBuilder WithTotalToday(long value)
  {
    _response.TotalToday = value;
    return this;
  }

  public StatsResponseBuilder WithTotalWeek(long value)
  {
    _response.TotalWeek = value;
    return this;
  }

  public StatsResponseBuilder WithTotalMonth(long value)
  {
    _response.TotalMonth = value;
    return this;
  }

  public StatsResponseBuilder WithHostName(string hostName)
  {
    _response.HostName = hostName;
    return this;
  }

  public StatsResponse Build() => _response;
}
