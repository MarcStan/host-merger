using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HostMerger
{
    public class HostMergerLogic
    {
        public async Task RunHostMergingAsync(Configuration config)
        {
            var readerWriter = new CloudBlobManager(config.AzureWebJobsStorage);

            var content = await readerWriter.ReadAsync<HostSource>(config.Sources);
            var existingHosts = await readerWriter.ReadLinesAsync(config.Sources);
            var blocklist = new Blocklist
            {
                Hostnames = new HashSet<string>(existingHosts)
            };
            blocklist = await AppendNewAsync(content, blocklist);

            await readerWriter.WriteAsync(config.Cache, blocklist.Hostnames.ToArray());

            var hostlist = blocklist.Build();

            await readerWriter.WriteAsync(config.Output, hostlist);
        }

        /// <summary>
        /// Given a set of host sources will iterate them and append them to the existing set of hosts.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="blocklist"></param>
        /// <returns></returns>
        public async Task<Blocklist> AppendNewAsync(HostSource content, Blocklist blocklist)
        {
            foreach (var host in content.Hosts)
            {
                blocklist.Hostnames.Add(host);
            }
            foreach (var host in content.Raw)
            {
                //format: "<ip> host"
                // can be ipv4 (e.g. 0.0.0.0) or ipv6 (e.g. ::)
                if (!host.Contains(" "))
                {
                    // TODO: log
                    continue;
                }
                var actualHost = host.Split(' ')[0];
            }

            var sources = await Task.WhenAll(content.Links.Select(LoadHostFileAsync));

            foreach (var src in sources)
            {
                // don't parallelize, otherwise we would need to synchronize the hashset!
                await AppendNewAsync(src, blocklist);
            }
            return blocklist;
        }

        private Task<HostSource> LoadHostFileAsync(string url)
        {
            throw new NotImplementedException();
        }
    }
}
