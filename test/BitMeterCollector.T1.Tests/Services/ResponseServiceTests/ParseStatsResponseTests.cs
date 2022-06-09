using System;
using BitMeterCollector.Shared.Models;
using BitMeterCollector.Shared.Services;
using BitMeterCollector.T1.Tests.TestSupport.Builders;
using NSubstitute;
using NUnit.Framework;
using Rn.NetCore.Common.Logging;

namespace BitMeterCollector.T1.Tests.Services.ResponseServiceTests;

[TestFixture]
public class ParseStatsResponseTests
{
  private const string ValidResponse = "611350574,25089372,5864387713,362364792,8893808604,553155335";
  private const string BadResponse = "a,b,c,d,e,f";
  private const string IncompleteEntry = "611350574,25089372,";

  [Test]
  public void ParseStatsResponse_GivenEmptyResponse_ShouldLog()
  {
    // arrange
    var logger = Substitute.For<ILoggerAdapter<ResponseService>>();
    var endPointConfig = BitMeterEndPointConfigBuilder.Default;

    var responseService = TestHelper.GetResponseService(logger);

    // act
    responseService.ParseStatsResponse(endPointConfig, string.Empty);

    // assert
    logger.Received(1).LogError("Empty response from {server}",
      endPointConfig.ServerName);
  }

  [Test]
  public void ParseStatsResponse_GivenEmptyResponse_ShouldReturnNull()
  {
    // arrange
    var endPointConfig = BitMeterEndPointConfigBuilder.Default;

    var responseService = TestHelper.GetResponseService();

    // act
    var mapped = responseService.ParseStatsResponse(endPointConfig, string.Empty);

    // assert
    Assert.That(mapped, Is.Null);
  }

  [Test]
  public void ParseStatsResponse_GivenIncompleteEntry_ShouldLog()
  {
    // arrange
    var logger = Substitute.For<ILoggerAdapter<ResponseService>>();
    var endPointConfig = BitMeterEndPointConfigBuilder.Default;

    var responseService = TestHelper.GetResponseService(logger);

    // act
    responseService.ParseStatsResponse(endPointConfig, IncompleteEntry);

    // assert
    logger.Received(1).LogError("Expecting 6 entries, got {count}", 2);
  }

  [Test]
  public void ParseStatsResponse_GivenIncompleteEntry_ShouldReturnNull()
  {
    // arrange
    var endPointConfig = BitMeterEndPointConfigBuilder.Default;

    var responseService = TestHelper.GetResponseService();

    // act
    var mapped = responseService.ParseStatsResponse(endPointConfig, IncompleteEntry);

    // assert
    Assert.That(mapped, Is.Null);
  }

  [Test]
  public void ParseStatsResponse_GivenNonNumericResponseValues_ShouldLog()
  {
    // arrange
    var logger = Substitute.For<ILoggerAdapter<ResponseService>>();
    var endPointConfig = BitMeterEndPointConfigBuilder.Default;

    var responseService = TestHelper.GetResponseService(logger);

    // act
    responseService.ParseStatsResponse(endPointConfig, BadResponse);

    // assert
    logger.Received(1).LogError(Arg.Any<Exception>(), "{type}: {message} | {stack}",
      "FormatException",
      Arg.Any<string>(),
      Arg.Any<string>());
  }

  [Test]
  public void ParseStatsResponse_GivenNonNumericResponseValues_ShouldReturnNull()
  {
    // arrange
    var endPointConfig = BitMeterEndPointConfigBuilder.Default;

    var responseService = TestHelper.GetResponseService();

    // act
    var mapped = responseService.ParseStatsResponse(endPointConfig, BadResponse);

    // assert
    Assert.That(mapped, Is.Null);
  }

  [Test]
  public void ParseStatsResponse_GivenValidResponse_ShouldReturnStatsResponse()
  {
    // arrange
    var endPointConfig = BitMeterEndPointConfigBuilder.Default;

    var responseService = TestHelper.GetResponseService();

    // act
    var mapped = responseService.ParseStatsResponse(endPointConfig, ValidResponse);

    // assert
    Assert.That(mapped, Is.InstanceOf<StatsResponse>());
  }

  [Test]
  public void ParseStatsResponse_GivenValidResponse_ShouldSetExpectedValues()
  {
    // arrange
    var endPointConfig = BitMeterEndPointConfigBuilder.Default;

    var responseService = TestHelper.GetResponseService();

    // act
    var mapped = responseService.ParseStatsResponse(endPointConfig, ValidResponse)!;

    // assert
    Assert.That(mapped.DownloadToday, Is.EqualTo(611350574));
    Assert.That(mapped.UploadToday, Is.EqualTo(25089372));
    Assert.That(mapped.DownloadWeek, Is.EqualTo(5864387713));
    Assert.That(mapped.UploadWeek, Is.EqualTo(362364792));
    Assert.That(mapped.DownloadMonth, Is.EqualTo(8893808604));
    Assert.That(mapped.UploadMonth, Is.EqualTo(553155335));
    Assert.That(mapped.TotalToday, Is.EqualTo(611350574 + 25089372));
    Assert.That(mapped.TotalWeek, Is.EqualTo(5864387713 + 362364792));
    Assert.That(mapped.TotalMonth, Is.EqualTo(8893808604 + 553155335));
  }
}
