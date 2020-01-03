using System.Threading.Tasks;

namespace BitMeterCollector.Services.Interfaces
{
  public interface IHttpService
  {
    Task<string> GetUrl(string url);
  }
}