using System;
using System.Collections.Generic;
using System.Text;

namespace HostMerger.Logic
{
    public class Blocklist
    {
        public HashSet<string> Hostnames { get; set; } = new HashSet<string>();

        public string Build()
        {
            var sb = new StringBuilder();

            sb.AppendLine("################################[INFO]################################");
            sb.Append("# Last update: ");
            sb.AppendLine(DateTime.UtcNow.ToString("G"));
            sb.AppendLine("# Supports IPv4 and IPv6");
            sb.Append("# Unique hostnames: ");
            sb.AppendLine(Hostnames.Count.ToString());
            sb.AppendLine("################################[HOSTS]################################");

            foreach (var host in Hostnames)
            {
                sb.Append("0.0.0.0 ");
                sb.AppendLine(host);
                sb.Append(":: ");
                sb.AppendLine(host);
            }

            return sb.ToString();
        }
    }
}
