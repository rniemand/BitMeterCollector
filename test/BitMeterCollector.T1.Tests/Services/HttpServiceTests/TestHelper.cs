using BitMeterCollector.Shared.Configuration;
using BitMeterCollector.Shared.Services;
using BitMeterCollector.T1.Tests.TestSupport.Builders;
using NSubstitute;
using Rn.NetCore.BasicHttp.Factories;

namespace BitMeterCollector.T1.Tests.Services.HttpServiceTests;

public static class TestHelper
{
  public static HttpService GetHttpService(
    BitMeterConfig? config = null,
    IHttpClientFactory? httpClientFactory = null)
  {
    return new HttpService(
      config ?? BitMeterConfigBuilder.Default,
      httpClientFactory ?? Substitute.For<IHttpClientFactory>());
  }
}
