using System.Threading;
using System.Threading.Tasks;
using BitMeterCollector.T1.Tests.TestSupport.Builders;
using NSubstitute;
using NUnit.Framework;
using Rn.NetCore.Metrics;

namespace BitMeterCollector.T1.Tests.Services.BitMeterCollectorTests;

[TestFixture]
public class TickAsyncTests
{
  [Test]
  public async Task TickAsync_GivenNoServers_ShouldDoNothing()
  {
    // arrange
    var metricService = Substitute.For<IMetricService>();
    var config = BitMeterConfigBuilder.Default;

    var collector = TestHelper.GetBitMeterCollector(
      metricService: metricService,
      config: config);

    // act
    await collector.TickAsync(CancellationToken.None);

    // assert
    await metricService.DidNotReceive().SubmitAsync(Arg.Any<CoreMetric>());
  }
}
