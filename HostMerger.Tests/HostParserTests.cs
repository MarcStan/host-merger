using FluentAssertions;
using HostMerger.Logic;
using NUnit.Framework;
using System;

namespace HostMerger.Tests
{
    public class HostParserTests
    {
        [Test]
        public void EmptyHostListWithCommentsShouldReturnEmpty()
        {
            var hostfile = "";
            var hosts = HostParser.Parse(hostfile);
            hosts.Should().HaveCount(0);
        }

        [Test]
        public void HostListWithMixedIpvShouldReturnHostNames()
        {
            var hostfile = string.Join(Environment.NewLine, new[]
            {
                "0.0.0.0 example.com",
                "::1 foobar.com",
                "# comment.com"
            });
            var hosts = HostParser.Parse(hostfile);
            hosts.Should().HaveCount(2);
            hosts.Should().ContainInOrder("example.com", "foobar.com");
        }

        [Test]
        public void HostListWithIpv6ShouldReturnHostNames()
        {
            var hostfile = string.Join(Environment.NewLine, new[]
            {
                ":: example.com",
                "::1 foobar.com",
                "# comment.com"
            });
            var hosts = HostParser.Parse(hostfile);
            hosts.Should().HaveCount(2);
            hosts.Should().ContainInOrder("example.com", "foobar.com");
        }

        [Test]
        public void HostListWithCommentsShouldReturnOnlyHostNames()
        {
            var hostfile = string.Join(Environment.NewLine, new[]
            {
                "0.0.0.0 example.com",
                "0.0.0.0 foobar.com",
                "# comment.com"
            });
            var hosts = HostParser.Parse(hostfile);
            hosts.Should().HaveCount(2);
            hosts.Should().ContainInOrder("example.com", "foobar.com");
        }

        [Test]
        public void HostListShouldReturnAllHostNames()
        {
            var hostfile = string.Join(Environment.NewLine, new[]
            {
                "0.0.0.0 example.com",
                "0.0.0.0 foobar.com",
                "0.0.0.0 blub.com"
            });
            var hosts = HostParser.Parse(hostfile);
            hosts.Should().HaveCount(3);
            hosts.Should().ContainInOrder("example.com", "foobar.com", "blub.com");
        }
    }
}
