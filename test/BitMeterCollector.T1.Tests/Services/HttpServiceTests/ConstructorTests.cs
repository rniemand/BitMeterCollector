using System;
using BitMeterCollector.T1.Tests.TestSupport.Builders;
using NSubstitute;
using NUnit.Framework;
using Rn.NetCore.BasicHttp.Factories;
using Rn.NetCore.BasicHttp.Wrappers;

namespace BitMeterCollector.T1.Tests.Services.HttpServiceTests;

[TestFixture]
public class ConstructorTests
{
  [Test]
  public void Constructor_GivenCalled_ShouldCreateNewHttpClient()
  {
    // arrange
    var httpClientFactory = Substitute.For<IHttpClientFactory>();

    // act
    TestHelper.GetHttpService(
      httpClientFactory: httpClientFactory);

    // assert
    httpClientFactory.Received(1).GetHttpClient();
  }

  [Test]
  public void Constructor_GivenCalled_ShouldSetHttpClientTimeout()
  {
    // arrange
    var httpClientFactory = Substitute.For<IHttpClientFactory>();
    var httpClient = Substitute.For<IHttpClient>();

    var config = new BitMeterConfigBuilder()
      .WithHttpServiceTimeoutMs(10)
      .Build();

    httpClientFactory.GetHttpClient().Returns(httpClient);

    // act
    TestHelper.GetHttpService(
      httpClientFactory: httpClientFactory,
      config: config);

    // assert
    httpClient.Received(1).Timeout = TimeSpan.FromMilliseconds(config.HttpServiceTimeoutMs);
  }
}
