using System;
using System.Threading.Tasks;

namespace HostMerger.Helper
{
    /// <summary>
    /// Provides better testability than HttpMessageHandler or IHttpClientFactory
    /// </summary>
    public class HttpClient : IHttpClient
    {
        private readonly System.Net.Http.HttpClient _httpClient;

        public HttpClient(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task<System.Net.Http.HttpResponseMessage> GetAsync(string url)
            => _httpClient.GetAsync(url);
    }
}
