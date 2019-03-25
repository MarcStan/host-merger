using System.Net.Http;
using System.Threading.Tasks;

namespace HostMerger.Helper
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string url);
    }
}
