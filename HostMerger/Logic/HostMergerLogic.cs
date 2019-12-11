using HostMerger.Config;
using HostMerger.Extensions;
using HostMerger.Helper;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// Processes the config and retrieves the files from storage.
        /// The creates a merged host file and outputs it to the output blob.
        /// </summary>
        /// <param name="cloudBlobManager"></param>
        /// <param name="outputCloudBlobManager"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task RunHostMergingAsync(ICloudBlobManager cloudBlobManager, ICloudBlobManager outputCloudBlobManager, Configuration config)
        {
            HostSource source;
            List<string> allowedHosts;
            using (_log.MeasureDuration("ReadWhitelist"))
            {
                source = await cloudBlobManager.ReadAsync<HostSource>(config.Source);
                if (source == null)
                    throw new FileNotFoundException($"Missing source file {config.Source} in container {cloudBlobManager.ContainerName}");

                var lines = await cloudBlobManager.ReadLinesAsync(config.Whitelist);
                if (lines == null)
                    throw new FileNotFoundException($"Missing whitelist {config.Whitelist} in container {cloudBlobManager.ContainerName}");
                allowedHosts = lines
                    .Where(l => !string.IsNullOrEmpty(l) || l.StartsWith("#"))
                    .ToList();
            }
            var blocklist = new Blocklist
            {
                Hostnames = new HashSet<string>()
            };
            using (_log.MeasureDuration("UpdateAll"))
            {
                blocklist = await AppendNewAsync(blocklist, source);
            }
            using (_log.MeasureDuration("ExcludeWhiltelistEntries"))
            {
                void remove(string s) => blocklist.Hostnames.Remove(s);
                allowedHosts.ForEach(remove);
            }

            string hostlist;
            using (_log.MeasureDuration("BuilldHostlist"))
                hostlist = blocklist.Build();

            using (_log.MeasureDuration("UploadHostlist"))
                await outputCloudBlobManager.WriteAsync(config.Output, hostlist);
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
                    _log.LogInformation($"Adding {src.Hosts.Length} hosts from {url}");
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
