using BitMeterCollector.Shared.Extensions;
using BitMeterCollector.Shared.Services;
using BitMeterCollector.T1.Tests.TestSupport.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using RnCore.Abstractions;
using RnCore.Logging;
using RnCore.Metrics;

namespace BitMeterCollector.T1.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
  [Test]
  public void AddBitMeterCollector_GivenCalled_ShouldRegisterHttpService()
  {
    // arrange
    var serviceCollection = GetServiceCollection();
    var configuration = Substitute.For<IConfiguration>();

    // act
    serviceCollection.AddBitMeterCollector(configuration);
    var provider = serviceCollection.BuildServiceProvider();

    // assert
    var service = provider.GetService<IHttpService>();
    Assert.That(service, Is.Not.Null);
    Assert.That(service, Is.InstanceOf<HttpService>());
  }

  [Test]
  public void AddBitMeterCollector_GivenCalled_ShouldRegisterResponseService()
  {
    // arrange
    var serviceCollection = GetServiceCollection();
    var configuration = Substitute.For<IConfiguration>();

    // act
    serviceCollection.AddBitMeterCollector(configuration);
    var provider = serviceCollection.BuildServiceProvider();

    // assert
    var service = provider.GetService<IResponseService>();
    Assert.That(service, Is.Not.Null);
    Assert.That(service, Is.InstanceOf<ResponseService>());
  }

  [Test]
  public void AddBitMeterCollector_GivenCalled_ShouldRegisterBitMeterCollector()
  {
    // arrange
    var configuration = Substitute.For<IConfiguration>();
    var serviceCollection = GetServiceCollection(configuration);
    
    // act
    serviceCollection.AddBitMeterCollector(configuration);
    var provider = serviceCollection.BuildServiceProvider();

    // assert
    var service = provider.GetService<IBitMeterCollector>();
    Assert.That(service, Is.Not.Null);
    Assert.That(service, Is.InstanceOf<Shared.Services.BitMeterCollector>());
  }

  [Test]
  public void AddBitMeterCollector_GivenCalled_ShouldRegisterDateTimeAbstraction()
  {
    // arrange
    var configuration = Substitute.For<IConfiguration>();
    var serviceCollection = GetServiceCollection();

    // act
    serviceCollection.AddBitMeterCollector(configuration);
    var provider = serviceCollection.BuildServiceProvider();

    // assert
    var service = provider.GetService<IDateTimeAbstraction>();
    Assert.That(service, Is.Not.Null);
    Assert.That(service, Is.InstanceOf<DateTimeAbstraction>());
  }

  [Test]
  public void AddBitMeterCollector_GivenCalled_ShouldRegisterLoggerAdapter()
  {
    // arrange
    var configuration = Substitute.For<IConfiguration>();
    var serviceCollection = GetServiceCollection();

    // act
    serviceCollection.AddBitMeterCollector(configuration);
    var provider = serviceCollection.BuildServiceProvider();

    // assert
    var service = provider.GetService<ILoggerAdapter<ServiceCollectionExtensionsTests>>();
    Assert.That(service, Is.Not.Null);
    Assert.That(service, Is.InstanceOf<LoggerAdapter<ServiceCollectionExtensionsTests>>());
  }

  [Test]
  public void AddBitMeterCollector_GivenCalled_ShouldRegisterMetricService()
  {
    // arrange
    var configuration = Substitute.For<IConfiguration>();
    var serviceCollection = GetServiceCollection(configuration);

    // act
    serviceCollection.AddBitMeterCollector(configuration);
    var provider = serviceCollection.BuildServiceProvider();

    // assert
    var service = provider.GetService<IMetricsService>();
    Assert.That(service, Is.Not.Null);
    Assert.That(service, Is.InstanceOf<MetricsService>());
  }
  
  private static IServiceCollection GetServiceCollection(IConfiguration? configuration = null)
  {
    var serviceCollection = new ServiceCollection()
      .AddSingleton(BitMeterConfigBuilder.Default);

    if(configuration is not null)
      serviceCollection.AddSingleton(configuration);

    return serviceCollection;
  }
}
