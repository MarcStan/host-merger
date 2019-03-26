using HostMerger.Config;
using HostMerger.Extensions;
using HostMerger.Helper;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _log;

        public HostMergerLogic(IHttpClient client, ILogger log)
        {
            _httpClient = client ?? throw new ArgumentNullException(nameof(client));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _policy = Policy.Handle<Exception>()
                .RetryAsync(3);
        }

        public async Task RunHostMergingAsync(ICloudBlobManager cloudBlobManager, Configuration config)
        {
            Blocklist blocklist;
            HostSource source;
            using (_log.MeasureDuration("ReadCache"))
            {
                source = await cloudBlobManager.ReadAsync<HostSource>(config.Sources);
                var existingHosts = await cloudBlobManager.ReadLinesAsync(config.Cache);
                blocklist = new Blocklist
                {
                    Hostnames = new HashSet<string>(existingHosts)
                };
            }
            using (_log.MeasureDuration("UpdateAll"))
            {
                blocklist = await AppendNewAsync(blocklist, source);
            }
            using (_log.MeasureDuration("WriteCache"))
                await cloudBlobManager.WriteAsync(config.Cache, blocklist.Hostnames.ToArray());

            string hostlist;
            using (_log.MeasureDuration("BuilldHostlist"))
                hostlist = blocklist.Build();

            using (_log.MeasureDuration("UploadHostlist"))
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
            foreach (var url in newSources.Links)
            {
                using (_log.MeasureDuration($"FetchHosts - {url}"))
                {
                    var src = await LoadHostFileAsync(url);
                    // don't parallelize, otherwise we would need to synchronize the hashset!
                    blocklist = await AppendNewAsync(blocklist, src);
                }
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
