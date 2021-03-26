using FluentAssertions;
using HostMerger.Logic;
using NUnit.Framework;
using System.Linq;

namespace HostMerger.Tests
{
    public class BlocklistTests
    {
        [Test]
        public void BuildingBlockListShouldPrintIPs()
        {
            var blocklist = new Blocklist();
            blocklist.Hostnames.Add("example.com");
            blocklist.Hostnames.Add("ads.com");

            var result = blocklist.Build();

            var lines = result.Split('\r', '\n');
            lines.Any(a => a == "0.0.0.0 example.com").Should().BeTrue();
            lines.Any(a => a == "0.0.0.0 example.com").Should().BeTrue();
            lines.Any(a => a == ":: ads.com").Should().BeTrue();
            lines.Any(a => a == ":: ads.com").Should().BeTrue();

            lines.Any(a => a.Contains("legit.com")).Should().BeFalse();
        }
    }
}
