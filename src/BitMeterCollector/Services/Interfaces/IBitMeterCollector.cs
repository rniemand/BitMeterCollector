using System.Threading.Tasks;

namespace BitMeterCollector.Services.Interfaces
{
  public interface IBitMeterCollector
  {
    Task Tick();
  }
}