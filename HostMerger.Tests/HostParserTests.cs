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
                "# comment.com",
                "| comment.com",
                "! comment.com"
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

        [TestCase("0.0.0.0 example.com #ad")]
        [TestCase("0.0.0.0 example.com # 0.0.0.0 foobar.com")]
        [TestCase("0.0.0.0\texample.com")]
        [TestCase("0.0.0.0\texample.com           ")]
        [TestCase("example.com")]
        [TestCase("https://example.com")]
        [TestCase("http://example.com")]
        public void EdgeCasesShouldWork(string line)
        {
            var hosts = HostParser.Parse(line);
            hosts.Should().HaveCount(1);
            hosts[0].Should().Be("example.com");
        }
    }
}
