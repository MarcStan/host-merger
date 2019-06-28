using System;
using System.Linq;

namespace HostMerger.Logic
{
    public static class HostParser
    {
        public static string[] Parse(string[] lines)
        {
            var comments = new[] { '#', '|', '!' };
            bool isComment(string line) => comments.Contains(line.First());

            // expecting "ip hostname" e.g. "0.0.0.0 ads.com", ":: ad.com"
            return lines
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(x => !isComment(x))
                .Select(x =>
                {
                    var p = x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    return p.Length > 1 ? p[1] : p[0];
                }).ToArray();
        }

        public static string[] Parse(string content)
        {
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return Parse(lines);
        }
    }
}
