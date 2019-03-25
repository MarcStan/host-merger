using System;
using System.Linq;

namespace HostMerger.Logic
{
    public static class HostParser
    {
        public static string[] Parse(string[] lines)
        {
            // expecting "ip hostname" e.g. "0.0.0.0 ads.com", ":: ad.com"
            return lines
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(x => !x.StartsWith("#") && x.Contains(" "))
                .Select(x => x.Split(' ')[1])
                .ToArray();
        }

        public static string[] Parse(string content)
        {
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return Parse(lines);
        }
    }
}
