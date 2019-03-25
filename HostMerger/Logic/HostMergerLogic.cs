using HostMerger.Helper;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HostMerger.Logic
{
    public class HostMergerLogic
    {
        private readonly AsyncRetryPolicy _policy;
        private readonly IHttpClient _httpClient;

        public HostMergerLogic(IHttpClient client)
        {
            _httpClient = client ?? throw new ArgumentNullException(nameof(client));

            _policy = Policy.Handle<Exception>()
                .RetryAsync(3);
        }

        public async Task RunHostMergingAsync(ICloudBlobManager cloudBlobManager, Configuration config)
        {
            var content = await cloudBlobManager.ReadAsync<HostSource>(config.Sources);
            var existingHosts = await cloudBlobManager.ReadLinesAsync(config.Sources);
            var blocklist = new Blocklist
            {
                Hostnames = new HashSet<string>(existingHosts)
            };
            blocklist = await AppendNewAsync(blocklist, content);

            await cloudBlobManager.WriteAsync(config.Cache, blocklist.Hostnames.ToArray());

            var hostlist = blocklist.Build();

            await cloudBlobManager.WriteAsync(config.Output, hostlist);
        }

        /// <summary>
        /// Given a set of host sources will iterate them and append them to the existing set of hosts and return the modified object.
        /// </summary>
        /// <param name="blocklist"></param>
        /// <param name="newSources"></param>
        /// <returns></returns>
        public async Task<Blocklist> AppendNewAsync(Blocklist blocklist, HostSource newSources)
        {
            foreach (var host in newSources.Hosts)
            {
                blocklist.Hostnames.Add(host);
            }
            foreach (var host in HostParser.Parse(newSources.Raw))
            {
                blocklist.Hostnames.Add(host);
            }

            var sources = await Task.WhenAll(newSources.Links.Select(LoadHostFileAsync));

            foreach (var src in sources)
            {
                // don't parallelize, otherwise we would need to synchronize the hashset!
                blocklist = await AppendNewAsync(blocklist, src);
            }
            return blocklist;
        }

        private async Task<HostSource> LoadHostFileAsync(string url)
        {
            var response = await _policy.ExecuteAndCaptureAsync(() => _httpClient.GetAsync(url));

            if (response.Outcome == OutcomeType.Failure)
                return null;
            var content = await response.Result.Content.ReadAsStringAsync();
            var rawHosts = HostParser.Parse(content);

            return new HostSource
            {
                Hosts = rawHosts
            };
        }
    }
}
