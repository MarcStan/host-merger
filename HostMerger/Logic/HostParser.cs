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
                .Where(x => !isComment(x) && (x.Contains(" ") || (x.Contains("\t"))))
                .Select(x => x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1])
                .ToArray();
        }

        public static string[] Parse(string content)
        {
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return Parse(lines);
        }
    }
}
