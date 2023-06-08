using BitMeterCollector.Shared.Services;
using NSubstitute;
using RnCore.Logging;

namespace BitMeterCollector.T1.Tests.Services.ResponseServiceTests;

public static class TestHelper
{
  public static ResponseService GetResponseService(ILoggerAdapter<ResponseService>? logger = null)
  {
    return new ResponseService(
      logger ?? Substitute.For<ILoggerAdapter<ResponseService>>());
  }
}
