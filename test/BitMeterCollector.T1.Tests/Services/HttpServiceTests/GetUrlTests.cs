using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Rn.NetCore.BasicHttp.Factories;
using Rn.NetCore.BasicHttp.Wrappers;

namespace BitMeterCollector.T1.Tests.Services.HttpServiceTests;

[TestFixture]
public class GetUrlTests
{
  private const string Url = "https://richardn.ca/";
  private const string ResponseBody = "ResponseBody";

  [Test]
  public async Task GetUrl_GivenCalled_ShouldWorkAsExpected()
  {
    // arrange
    var httpClientFactory = Substitute.For<IHttpClientFactory>();
    var httpClient = Substitute.For<IHttpClient>();

    httpClientFactory
      .GetHttpClient()
      .Returns(httpClient);

    httpClient
      .SendAsync(Arg.Any<HttpRequestMessage>())
      .Returns(new HttpResponseMessage
      {
        Content = new StringContent(ResponseBody)
      });

    var httpService = TestHelper.GetHttpService(httpClientFactory: httpClientFactory);

    // act
    var response = await httpService.GetUrl(Url);

    // assert
    await httpClient.Received(1).SendAsync(Arg.Is<HttpRequestMessage>(x =>
      x.Method == HttpMethod.Get &&
      x.RequestUri!.AbsoluteUri == Url));

    Assert.That(response, Is.EqualTo(ResponseBody));
  }
}
