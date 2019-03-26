using FluentAssertions;
using HostMerger.Helper;
using HostMerger.Logic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HostMerger.Tests
{
    public class HostMergerLogicTests
    {
        [Test]
        public async Task AppendRawToEmptyBlocklist()
        {
            var merger = new HostMergerLogic(new Mock<IHttpClient>().Object, new Mock<ILogger>().Object);

            var blocklist = new Blocklist();
            var sources = new HostSource
            {
                Raw = new[]
                {
                    ":: example.com",
                    ":: foobar.com"
                }
            };

            blocklist = await merger.AppendNewAsync(blocklist, sources);

            blocklist.Hostnames.Should().HaveCount(2);
            blocklist.Hostnames.Should().ContainInOrder("example.com", "foobar.com");
        }

        [Test]
        public async Task AppendRawToBlocklist()
        {
            var merger = new HostMergerLogic(new Mock<IHttpClient>().Object, new Mock<ILogger>().Object);

            var blocklist = new Blocklist
            {
                Hostnames = new HashSet<string>(new[]
                {
                    "blub.com"
                })
            };
            var sources = new HostSource
            {
                Raw = new[]
                {
                    ":: example.com",
                    ":: foobar.com"
                }
            };

            blocklist = await merger.AppendNewAsync(blocklist, sources);

            blocklist.Hostnames.Should().HaveCount(3);
            blocklist.Hostnames.Should().ContainInOrder("blub.com", "example.com", "foobar.com");
        }

        [Test]
        public async Task AppendRawToBlocklistShouldMergeDuplicates()
        {
            var merger = new HostMergerLogic(new Mock<IHttpClient>().Object, new Mock<ILogger>().Object);

            var blocklist = new Blocklist
            {
                Hostnames = new HashSet<string>(new[]
                {
                    "example.com"
                })
            };
            var sources = new HostSource
            {
                Raw = new[]
                {
                    ":: example.com",
                    ":: foobar.com"
                }
            };

            blocklist = await merger.AppendNewAsync(blocklist, sources);

            blocklist.Hostnames.Should().HaveCount(2);
            blocklist.Hostnames.Should().ContainInOrder("example.com", "foobar.com");
        }

        [Test]
        public async Task AppendHostsToEmptyBlocklist()
        {
            var merger = new HostMergerLogic(new Mock<IHttpClient>().Object, new Mock<ILogger>().Object);

            var blocklist = new Blocklist();
            var sources = new HostSource
            {
                Hosts = new[]
                {
                    "example.com",
                    "foobar.com"
                }
            };

            blocklist = await merger.AppendNewAsync(blocklist, sources);

            blocklist.Hostnames.Should().HaveCount(2);
            blocklist.Hostnames.Should().ContainInOrder("example.com", "foobar.com");
        }

        [Test]
        public async Task AppendHostsToBlocklist()
        {
            var merger = new HostMergerLogic(new Mock<IHttpClient>().Object, new Mock<ILogger>().Object);

            var blocklist = new Blocklist
            {
                Hostnames = new HashSet<string>(new[]
                {
                    "blub.com"
                })
            };
            var sources = new HostSource
            {
                Hosts = new[]
                {
                    "example.com",
                    "foobar.com"
                }
            };

            blocklist = await merger.AppendNewAsync(blocklist, sources);

            blocklist.Hostnames.Should().HaveCount(3);
            blocklist.Hostnames.Should().ContainInOrder("blub.com", "example.com", "foobar.com");
        }

        [Test]
        public async Task AppendHostsToBlocklistShouldMergeDuplicates()
        {
            var merger = new HostMergerLogic(new Mock<IHttpClient>().Object, new Mock<ILogger>().Object);

            var blocklist = new Blocklist
            {
                Hostnames = new HashSet<string>(new[]
                {
                    "example.com"
                })
            };
            var sources = new HostSource
            {
                Hosts = new[]
                {
                    "example.com",
                    "foobar.com"
                }
            };

            blocklist = await merger.AppendNewAsync(blocklist, sources);

            blocklist.Hostnames.Should().HaveCount(2);
            blocklist.Hostnames.Should().ContainInOrder("example.com", "foobar.com");
        }

        [Test]
        public async Task AppendLinkToEmptyBlocklist()
        {
            const string url = "http://example.com/fakehosts.txt";
            var clientMock = new Mock<IHttpClient>();
            var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            fakeResponse.Content = new StringContent(string.Join(Environment.NewLine, new[] { "# fake.com", "0.0.0.0 example.com", ":: foobar.com" }));

            clientMock.Setup(x => x.GetAsync(url))
                .Returns(Task.FromResult(fakeResponse));

            var merger = new HostMergerLogic(clientMock.Object, new Mock<ILogger>().Object);

            var blocklist = new Blocklist();
            var sources = new HostSource
            {
                Links = new[]
                {
                    url
                }
            };

            blocklist = await merger.AppendNewAsync(blocklist, sources);

            blocklist.Hostnames.Should().HaveCount(2);
            blocklist.Hostnames.Should().ContainInOrder("example.com", "foobar.com");
        }

        [Test]
        public async Task AppendLinkToBlocklist()
        {
            const string url = "http://example.com/fakehosts.txt";
            var clientMock = new Mock<IHttpClient>();
            var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            fakeResponse.Content = new StringContent(string.Join(Environment.NewLine, new[] { "# fake.com", "0.0.0.0 example.com", ":: foobar.com" }));

            clientMock.Setup(x => x.GetAsync(url))
                .Returns(Task.FromResult(fakeResponse));

            var merger = new HostMergerLogic(clientMock.Object, new Mock<ILogger>().Object);

            var blocklist = new Blocklist
            {
                Hostnames = new HashSet<string>(new[]
                {
                    "blub.com"
                })
            };
            var sources = new HostSource
            {
                Links = new[]
                {
                    url
                }
            };

            blocklist = await merger.AppendNewAsync(blocklist, sources);

            blocklist.Hostnames.Should().HaveCount(3);
            blocklist.Hostnames.Should().ContainInOrder("blub.com", "example.com", "foobar.com");
        }

        [Test]
        public async Task AppendLinkToBlocklistShouldMergeDuplicates()
        {
            const string url = "http://example.com/fakehosts.txt";
            var clientMock = new Mock<IHttpClient>();
            var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            fakeResponse.Content = new StringContent(string.Join(Environment.NewLine, new[] { "# fake.com", "0.0.0.0 example.com", ":: foobar.com" }));

            clientMock.Setup(x => x.GetAsync(url))
                .Returns(Task.FromResult(fakeResponse));

            var merger = new HostMergerLogic(clientMock.Object, new Mock<ILogger>().Object);

            var blocklist = new Blocklist
            {
                Hostnames = new HashSet<string>(new[]
                {
                    "example.com"
                })
            };
            var sources = new HostSource
            {
                Links = new[]
                {
                    url
                }
            };

            blocklist = await merger.AppendNewAsync(blocklist, sources);

            blocklist.Hostnames.Should().HaveCount(2);
            blocklist.Hostnames.Should().ContainInOrder("example.com", "foobar.com");
        }
    }
}
